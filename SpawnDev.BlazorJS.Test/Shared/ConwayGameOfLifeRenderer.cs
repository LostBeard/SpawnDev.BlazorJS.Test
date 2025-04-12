using SpawnDev.BlazorJS.JSObjects;
using SpawnDev.BlazorJS.Test.GameOfLife;
using SpawnDev.BlazorJS.WebWorkers;
using System.Diagnostics;
using Timer = System.Timers.Timer;

namespace SpawnDev.BlazorJS.Test.Shared
{
    public class ConwayGameOfLifeRenderer : IConwayGameOfLifeRenderer
    {
        HTMLCanvasElement? Canvas { get; set; } = null;
        OffscreenCanvas? OffscreenCanvas { get; set; } = null;
        CanvasRenderingContext2D ctx { get; set; }
        Timer? Timer = null;
        public int TileSize { get; set; } = 4;
        public int LegendColorSize => 16;
        public int BoardRowWidth => TileSize * Board.Width;
        public int BoardRowHeight => TileSize * Board.Height;
        public Board Board { get; private set; } = new Board();
        public bool Paused { get; set; } = false;
        public TimeSpan drawTime { get; set; } = new TimeSpan();
        public ConwayGameOfLifeRenderer([FromServices] BlazorJSRuntime js)
        {
            js.Log($"Woohoo!: {js.GlobalScope.ToString()}");
            Timer = new Timer();
            Timer.Elapsed += Timer_Elapsed;
            Timer.Interval = 5;
            Timer.AutoReset = false;
            Board.OnTicked += Board_OnTicked;
            Board.OnReset += Board_OnReset;
            Board.OnChanged += Board_OnChanged;
            Timer.Enabled = true;
        }
        public ConwayGameOfLifeRenderer(OffscreenCanvas offscreenCanvas, int width, int height, double startingPopulation)
        {
            Timer = new Timer();
            Timer.Elapsed += Timer_Elapsed;
            Timer.Interval = 5;
            Timer.AutoReset = false;
            Board.OnTicked += Board_OnTicked;
            Board.OnReset += Board_OnReset;
            Board.OnChanged += Board_OnChanged;
            Timer.Enabled = true;
            OffscreenCanvas = offscreenCanvas;
            ctx = OffscreenCanvas.Get2DContext();
            Board.Resize(width, height, startingPopulation);
            ResetCanvas();
        }

        private void Board_OnChanged(ChangedCell change)
        {
            var fillStyle = ctx.FillStyle;
            var x = change.X;
            var y = change.Y;
            var cx = x * TileSize;
            var cy = y * TileSize;
            var cellValue = change.Value;
            if (!Colors.TryGetValue(cellValue, out var bgColor))
            {
                bgColor = Colors[BoardTileState.Void];
            }
            if (fillStyle != bgColor)
            {
                ctx.FillStyle = bgColor;
                fillStyle = bgColor;
            }
            ctx.FillRect(cx, cy, TileSize, TileSize);
        }

        public async Task Init(OffscreenCanvas offscreenCanvas, int width, int height, double startingPopulation)
        {
            OffscreenCanvas = offscreenCanvas;
            ctx = OffscreenCanvas.Get2DContext();
            await ResizeBoard(width, height, startingPopulation);
            ResetCanvas();
        }
        public async Task Init(HTMLCanvasElement canvas, int width, int height, double startingPopulation)
        {
            Canvas = canvas;
            ctx = Canvas.Get2DContext();
            await ResizeBoard(width, height, startingPopulation);
            ResetCanvas();
        }
        public async Task<GenerationStat> GetStats() => Board.GenerationStat;
        public async Task ResizeBoard(int width, int height, double startingPopulation)
        {
            Board.Resize(width, height, startingPopulation);
        }
        public async Task ResetBoard(double startingPopulation)
        {
            Board.Reset(startingPopulation);
        }
        public async Task ResizeBoard(int width, int height)
        {
            Board.Resize(width, height);
        }
        public async Task ResetBoard()
        {
            Board.Reset();
        }
        public async Task Snapshot()
        {
            var snapshot = Board.SaveSnapShot();
            Console.WriteLine(snapshot);
        }
        public async Task Reset()
        {
            Board.Reset();
        }
        public async Task TogglePaused()
        {
            Paused = !Paused;
        }
        public async Task Unpause()
        {
            Paused = false;
        }
        public async Task Pause()
        {
            Paused = true;
        }
        public async Task Tick()
        {
            Board?.Tick();
        }
        public int CanvasWidth { get; private set; }
        public int CanvasHeight { get; private set; }
        private void ResetCanvas()
        {
            CanvasWidth = TileSize * Board.Width;
            CanvasHeight = TileSize * Board.Height;
            if (Canvas != null)
            {
                Canvas.Width = CanvasWidth;
                Canvas.Height = CanvasHeight;
            }
            if (OffscreenCanvas != null)
            {
                OffscreenCanvas.Width = CanvasWidth;
                OffscreenCanvas.Height = CanvasHeight;
            }
            ctx.FillStyle = Colors[BoardTileState.Void];
            ctx.FillRect(0, 0, CanvasWidth, CanvasHeight);
        }
        private void Board_OnReset()
        {
            if (ctx == null) return;
            ResetCanvas();
        }
        public static Dictionary<BoardTileState, string> Colors = new Dictionary<BoardTileState, string> {
            { BoardTileState.Void, "#000000" },
            { BoardTileState.Birth, "#0062ff" },
            { BoardTileState.Youngling, "#2e7eff" },
            { BoardTileState.Adult, "#73a9ff" },
            { BoardTileState.Elder, "#b8d3ff" },
            { BoardTileState.DeathUnderPopulation, "#3b0d3b" },
            { BoardTileState.DeathOverPopulation, "#3b0d13"},
            //
            { BoardTileState.Alive, "#1fab3d" },
            { BoardTileState.Dead, "#5a5c2f" },
        };
        private void Board_OnTicked(GenerationStat generationStat)
        {
            if (ctx == null) return;
            var sw = Stopwatch.StartNew();
            if (generationStat.Changed.Count > 0)
            {
                var fillStyle = ctx.FillStyle;
                foreach (var change in generationStat.Changed)
                {
                    var x = change.X;
                    var y = change.Y;
                    var cx = x * TileSize;
                    var cy = y * TileSize;
                    var cellValue = change.Value;
                    if (!Colors.TryGetValue(cellValue, out var bgColor))
                    {
                        bgColor = Colors[BoardTileState.Void];
                    }
                    if (fillStyle != bgColor)
                    {
                        ctx.FillStyle = bgColor;
                        fillStyle = bgColor;
                    }
                    ctx.FillRect(cx, cy, TileSize, TileSize);
                }
            }
            sw.Stop();
            drawTime = sw.Elapsed;
        }
        private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (IsDisposed || Timer == null) return;
            Timer.Enabled = false;
            if (!Paused) Board.Tick();
            Timer.Start();
        }
        public bool IsDisposed = false;
        public ValueTask DisposeAsync()
        {
            if (IsDisposed) return ValueTask.CompletedTask;
            IsDisposed = true;
            Timer?.Dispose();
            Timer = null;
            ctx.Dispose();
            Canvas?.Dispose();
            Canvas = null;
            OffscreenCanvas?.Dispose();
            OffscreenCanvas = null;
            return ValueTask.CompletedTask;
        }
    }
}
