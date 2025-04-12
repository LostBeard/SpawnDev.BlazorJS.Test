using Microsoft.AspNetCore.Components;
using SpawnDev.BlazorJS.JSObjects;
using SpawnDev.BlazorJS.Test.GameOfLife;
using SpawnDev.BlazorJS.WebWorkers;
using Timer = System.Timers.Timer;

namespace SpawnDev.BlazorJS.Test.Shared
{
    public partial class ConwayGameOfLife : IAsyncDisposable
    {
        HTMLCanvasElement? canvas = null;
        ElementReference CanvasElRef { get; set; }
        Timer? Timer = null;
        int LegendColorSize => 16;
        int BoardWidth { get; set; } = 128;
        int BoardHeight { get; set; } = 128;
        double StartingPopulation = 50;
        bool Paused = false;
        TimeSpan drawTime = new TimeSpan();
        ServiceCallDispatcher? worker = null;
        GenerationStat GenerationStat = new GenerationStat();
        [Inject]
        WebWorkerService WebWorkerService { get; set; }
        [Inject]
        BlazorJSRuntime JS { get; set; }
        [Parameter]
        public bool UseWorker { get; set; } = false;
        bool UseLocalWorker = false;
        IConwayGameOfLifeRenderer? renderer = null;
        string RendererServiceKey = Guid.NewGuid().ToString();
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (canvas == null)
            {
                canvas = new HTMLCanvasElement(CanvasElRef);
                var workerSupported = WebWorkerService.WebWorkerSupported;
                if (UseWorker && workerSupported)
                {
                    if (UseLocalWorker)
                    {
                        // this option will still use a dispatcher, but runs on the window and not in a worker
                        worker = WebWorkerService.Local;
                    }
                    else
                    {
                        // get a new worker for the renderer
                        worker = await WebWorkerService.GetWebWorker();
                    }
                }
                if (UseWorker && worker != null)
                {
                    if (false)
                    {
                        // below creates a new ConwayGameOfLifeRenderer instance as keyed service in the worker
                        await worker.New<IConwayGameOfLifeRenderer>(RendererServiceKey, () => new ConwayGameOfLifeRenderer(null));
                        // get an interface proxy to the instance we just created on the worker
                        renderer = worker.GetKeyedService<IConwayGameOfLifeRenderer>(RendererServiceKey);
                        // get the offscreen canvas to sen to the worker
                        var offscreenCanvas = canvas.TransferControlToOffscreen();
                        // initialize the worker
                        await renderer.Init(offscreenCanvas, BoardWidth, BoardHeight, StartingPopulation);
                    }
                    else
                    {
                        // get the offscreen canvas to sen to the worker
                        var offscreenCanvas = canvas.TransferControlToOffscreen();
                        // below creates a new ConwayGameOfLifeRenderer instance as keyed service in the worker
                        await worker.New<IConwayGameOfLifeRenderer>(RendererServiceKey, () => new ConwayGameOfLifeRenderer(offscreenCanvas, BoardWidth, BoardHeight, StartingPopulation));
                        // get an interface proxy to the instance we just created on the worker
                        renderer = worker.GetKeyedService<IConwayGameOfLifeRenderer>(RendererServiceKey);
                    }
                }
                else
                {
                    // create the renderer directly
                    renderer = new ConwayGameOfLifeRenderer(JS);
                    // init the renderer using the canvas
                    await renderer.Init(canvas, BoardWidth, BoardHeight, StartingPopulation);
                }
            }
            Timer = new Timer();
            Timer.Elapsed += Timer_Elapsed;
            Timer.Interval = 300;
            Timer.AutoReset = false;
            Timer.Start();
        }
        async Task Tick()
        {
            if (renderer == null) return;
            await renderer.Tick();
        }
        async Task Snapshot()
        {
            if (renderer == null) return;
            await renderer.Snapshot();
        }
        async Task Reset()
        {
            if (renderer == null) return;
            await renderer.Reset();
        }
        async Task TogglePaused()
        {
            if (renderer == null) return;
            if (Paused)
            {
                await renderer.Unpause();
                Paused = !Paused;
            }
            else
            {
                await renderer.Pause();
                Paused = !Paused;
            }
        }
        private async void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (IsDisposed || Timer == null) return;
            Timer.Enabled = false;
            if (!Paused && renderer != null)
            {
                try
                {
                    GenerationStat = await renderer.GetStats();
                    StateHasChanged();
                }
                catch { }
            }
            if (!IsDisposed) Timer?.Start();
        }
        bool IsDisposed = false;
        public async ValueTask DisposeAsync()
        {
            if (IsDisposed) return;
            IsDisposed = true;
            Timer?.Dispose();
            Timer = null;
            if (renderer != null)
            {
                await renderer.DisposeAsync();
                renderer = null;
            }
            if (worker != null)
            {
                await worker.RemoveKeyedService<IConwayGameOfLifeRenderer>(RendererServiceKey);
                if (!UseLocalWorker)
                {
                    worker.Dispose();
                }
                worker = null;
            }
            canvas?.Dispose();
            canvas = null;
        }
    }
}
