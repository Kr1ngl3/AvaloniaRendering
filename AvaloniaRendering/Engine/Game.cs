using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Threading;
using AvaloniaRendering.Controls;
using AvaloniaRendering.Engine.Scenes;
using ObjLoader.Loader.Data.VertexData;
using ObjLoader.Loader.Loaders;
using SkiaSharp;
using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Numerics;
using System.Reactive.Concurrency;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;


namespace AvaloniaRendering.Engine;

class Game
{
    const int FPS = 60;
    const float DeltaTime = 1f / FPS; 

    private readonly RenderingView _renderingView;
    private readonly Graphics _graphics;
    //private readonly Timer _timer;

    private Scene _currentScene;

    private bool _isKill = false;

    public double Frames { get; private set; }

    public Game(RenderingView renderingView)
    {
        _renderingView = renderingView;
        _graphics = new Graphics(renderingView);

        //_timer = new Timer(1d / FPS * 1000);
        //_timer.Elapsed += Go;
        _currentScene = new DoubleCubeScene(_graphics, new Transformer((int)renderingView.Width, (int)renderingView.Height));
    }

    public void Start()
    {
        //_timer.Start();
        Task.Run(() =>
        {
            int counter = 0;
            // Create a new Stopwatch instance
            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();
            while (true)
            {


                Go();
                // Stop the stopwatch after the iteration

                if (counter == 100)
                {

                    stopwatch.Stop();
                    counter = 0;
                    Dispatcher.UIThread.Invoke(() => _renderingView.Text.Text = (100000 / stopwatch.Elapsed.TotalMilliseconds).ToString() + " fps");

                    stopwatch.Reset();

                    stopwatch.Start();
                }

                counter++;
            }
        });
    }

    private void Go(/*object? sender, ElapsedEventArgs eventArgs*/)
    {
        _graphics.BeginFrame();
        UpdateModel();

        if (_isKill)
            return;

        ComposeFrame();
        _graphics.EndFrame();

    }

    private void UpdateModel()
    {
        if (_renderingView.KeyMap[Key.Escape])
            Kill();

        _currentScene.Update(_renderingView, DeltaTime);
    }

    private void ComposeFrame()
    {
        _currentScene.Draw();
    }

    private void Kill()
    {
        //_timer.Stop();
        //_timer.Elapsed -= Go;
        _renderingView.End();
        _isKill = true;
    }

    private (LoadResult, (int, int)[]) WireFrame3D()
    {
        var objLoaderFactory = new ObjLoaderFactory();
        var objLoader = objLoaderFactory.Create();
        var fileStream = AssetLoader.Open(new Uri("avares://AvaloniaRendering/Assets/cube.txt"));
        var result = objLoader.Load(fileStream);

        (int, int)[] lineList =
            [(0, 1), (1,2), (2,3), (3,0), // front
            (4,5), (5,6), (6,7), (7,4), // back
            (3,7), (2,6), (1,5), (0,4)]; // sides

        return (result, lineList);
    }
}
