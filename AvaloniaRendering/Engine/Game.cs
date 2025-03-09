using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform;
using AvaloniaRendering.Controls;
using ObjLoader.Loader.Data.VertexData;
using ObjLoader.Loader.Loaders;
using SkiaSharp;
using System;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Timers;


namespace AvaloniaRendering.Engine;

class Game
{
    const int FPS = 60;
    const float DeltaTime = 1f / FPS; 

    private readonly RenderingView _renderingView;
    private readonly Graphics _graphics;
    private readonly Timer _timer;

    private Scene _currentScene;

    private bool _isKill = false;

    public Game(RenderingView renderingView)
    {
        _renderingView = renderingView;
        _graphics = new Graphics(renderingView);

        _timer = new Timer(1d / FPS * 1000);
        _timer.Elapsed += Go;
        _currentScene = new CubeScene(_graphics, new Transformer((int)renderingView.Width, (int)renderingView.Height));
    }

    public void Start()
    {
        _timer.Start();
    }

    private void Go(object? sender, ElapsedEventArgs eventArgs)
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
        _timer.Stop();
        _timer.Elapsed -= Go;
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
