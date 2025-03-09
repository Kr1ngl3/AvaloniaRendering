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
using System.Timers;


namespace AvaloniaRendering.Engine;

class Game
{
    const int FPS = 60;
    const float RotateSpeed = MathF.PI / FPS;

    private readonly Rendere _rendere;
    private readonly Graphics _graphics;
    private readonly Timer _timer;

    private float _yaw;
    private float _pitch;
    private float _roll;

    private readonly int _width;
    private readonly int _height;

    (Vector3[] Vertices, (int, int, int)[] Faces) _model;

    public Game(Rendere rendere)
    {
        _width = (int)rendere.Width;
        _height = (int)rendere.Height;

        _rendere = rendere;
        _graphics = new Graphics(rendere);

        _model = Model3D();

        _timer = new Timer(1d / FPS * 1000);
        _timer.Elapsed += Go;
    }

    public void Start()
    {
        _timer.Start();
    }

    private void Go(object? sender, ElapsedEventArgs eventArgs)
    {
        _graphics.BeginFrame();
        UpdateModel();
        ComposeFrame();
        _graphics.EndFrame();

    }

    private void UpdateModel()
    {
        _yaw += _rendere.KeyMap[Key.A] ? RotateSpeed : 0;
        _yaw -= _rendere.KeyMap[Key.D] ? RotateSpeed : 0;

        _pitch += _rendere.KeyMap[Key.W] ? RotateSpeed : 0;
        _pitch -= _rendere.KeyMap[Key.S] ? RotateSpeed : 0;

        _roll += _rendere.KeyMap[Key.Q] ? RotateSpeed : 0;
        _roll -= _rendere.KeyMap[Key.E] ? RotateSpeed : 0;
    }

    private void ComposeFrame()
    {
        Span<Vector3> vertices = stackalloc Vector3[_model.Vertices.Length];
        _model.Vertices.CopyTo(vertices);


        // move 2 back to move away from screen which is at z=1
        Matrix4x4 matrix = Matrix4x4.CreateFromYawPitchRoll(_yaw, _pitch, _roll) * Matrix4x4.CreateTranslation(new Vector3(0, 0, 2));

        // transform from model space to view/world space
        for (int i = 0; i < vertices.Length; i++)
        {
            //vertices[i] = Vector3.Transform(vertices[i], Matrix4x4.CreateScale(new Vector3(1f/5)));
            vertices[i] = Vector3.Transform(vertices[i], matrix);
        }


        for (int i = 0; i < _model.Faces.Length; i++)
        {
            Vector3 v0 = vertices[_model.Faces[i].Item1 - 1];
            Vector3 v1 = vertices[_model.Faces[i].Item2 - 1];
            Vector3 v2 = vertices[_model.Faces[i].Item3 - 1];

            if (Dot(Cross(v1 - v0, v2 - v0), v0) >= 0)
                continue;

            _graphics.DrawTriangle(
                SKColor.FromHsv(i * 30 % 360, 100, 100),
                Transform(v0),
                Transform(v1),
                Transform(v2));
        }
    }

    private void Kill()
    {
        _timer.Stop();
        _timer.Elapsed -= Go;
    }

    private Vector2 Transform(Vector3 vertex)
    {
        float distToScreen = 1;
        float inverse = distToScreen / vertex.Z;
        float xFactor = _width / 2;
        float yFactor = _height / 2;

        return new((vertex.X * inverse + 1) * xFactor, (-vertex.Y * inverse + 1) * yFactor);
    }

    private (Vector3[] Vertices, (int, int, int)[] Faces) Model3D()
    {
        var objLoaderFactory = new ObjLoaderFactory();
        var objLoader = objLoaderFactory.Create();
        using var fileStream = AssetLoader.Open(new Uri("avares://AvaloniaRendering/Assets/cube.txt"));
        var result = objLoader.Load(fileStream);

        return (result.Vertices.Select(VertexToVector).ToArray(),
            result.Groups[0].Faces.Select(face => (face[0].VertexIndex, face[1].VertexIndex, face[2].VertexIndex)).ToArray());
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

    private Vector3 VertexToVector(Vertex vertex) => new Vector3(vertex.X, vertex.Y, vertex.Z);

    private Vector3 Cross(Vector3 lhs, Vector3 rhs)
    {
        return new Vector3(
            lhs.Y * rhs.Z - lhs.Z * rhs.Y,
            -(lhs.X * rhs.Z - lhs.Z * rhs.X),
            lhs.X * rhs.Y - lhs.Y * rhs.X);
    }

    private float Dot(Vector3 lhs, Vector3 rhs)
    {
        return lhs.X * rhs.X + lhs.Y * rhs.Y + lhs.Z * rhs.Z;
    }

}
