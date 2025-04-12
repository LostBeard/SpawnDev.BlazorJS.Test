using SpawnDev.BlazorJS.JSObjects;
using SpawnDev.BlazorJS.Test.GameOfLife;

namespace SpawnDev.BlazorJS.Test.Shared
{
    public interface IConwayGameOfLifeRenderer : IAsyncDisposable
    {
        Board Board { get; }
        int BoardRowHeight { get; }
        int BoardRowWidth { get; }
        int CanvasHeight { get; }
        int CanvasWidth { get; }
        TimeSpan drawTime { get; set; }
        int LegendColorSize { get; }
        bool Paused { get; set; }
        int TileSize { get; set; }

        Task<GenerationStat> GetStats();
        Task Init(HTMLCanvasElement canvas, int width, int height, double startingPopulation);
        Task Init(OffscreenCanvas offscreenCanvas, int width, int height, double startingPopulation);
        Task Pause();
        Task Reset();
        Task ResetBoard();
        Task ResetBoard(double startingPopulation);
        Task ResizeBoard(int width, int height);
        Task ResizeBoard(int width, int height, double startingPopulation);
        Task Snapshot();
        Task Tick();
        Task TogglePaused();
        Task Unpause();
    }
}