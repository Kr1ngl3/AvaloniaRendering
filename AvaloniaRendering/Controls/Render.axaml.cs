using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using ObjLoader.Loader.Loaders;
using System.Numerics;
using System;
using Avalonia.Media.Imaging;
using System.Runtime.InteropServices;
using ObjLoader.Loader.Data.VertexData;
using System.Linq;
using System.Collections.Generic;
using Avalonia.Input;
using System.Timers;
using Avalonia.Threading;

namespace Rendering.Controls;
public partial class Render : UserControl
{
    const float RotateSpeed = MathF.PI / FPS;
    const int FPS = 144;
    private static readonly Key[] UsedKeys = { Key.W, Key.S, Key.Q, Key.E, Key.A, Key.D };

    private readonly int _width;
    private readonly int _height;
    private readonly byte[] _data;

    private float _yaw;
    private float _pitch;
    private float _roll;

    Dictionary<Key, bool> keyMap = new();
    public Render()
    {
        InitializeComponent();

        _width = (int)Width;
        _height = (int)Height;
        _data = new byte[_width * _height * 4];

        foreach (Key key in UsedKeys)
            keyMap[key] = false;

        KeyDownEvent.AddClassHandler<TopLevel>(OnKeyDown, handledEventsToo: true);
        KeyUpEvent.AddClassHandler<TopLevel>(OnKeyUp, handledEventsToo: true);
        Initialize();
    }

    private void Initialize()
    {
        var bitmap = new WriteableBitmap(
            new PixelSize(_width, _height),
            new Avalonia.Vector(96, 96),
        PixelFormat.Rgb32,
        AlphaFormat.Premul);
        image.Source = bitmap;
        var frameBuffer = bitmap.Lock();

        (LoadResult result, (int, int)[] lineList) cube = Model3D();

        Vector3[] originalVertices = cube.result.Vertices.Select(v => VertexToVector(v)).ToArray();

        Timer timer = new Timer(1d / FPS * 1000);
        timer.Elapsed += (sender, eventArgs) =>
        {
            Span<Vector3> vertices = stackalloc Vector3[originalVertices.Length];
            originalVertices.CopyTo(vertices);
            Compose(frameBuffer, vertices, cube.lineList, HsvColor.ToRgb(eventArgs.SignalTime.TimeOfDay.TotalMilliseconds / 10 % 360, 1, 1));
        };
        timer.Start();
    }

    private void Compose(ILockedFramebuffer frameBuffer, Span<Vector3> vertices, (int, int)[] lineList, Color color)
    {
        Color background = HsvColor.ToRgb(0, 1, 0);

        Array.Fill<byte>(_data, 0);

        _yaw += keyMap[Key.A] ? RotateSpeed : 0;
        _yaw -= keyMap[Key.D] ? RotateSpeed : 0;

        _pitch += keyMap[Key.W] ? RotateSpeed : 0;
        _pitch -= keyMap[Key.S] ? RotateSpeed : 0;

        _roll += keyMap[Key.Q] ? RotateSpeed : 0;
        _roll -= keyMap[Key.E] ? RotateSpeed : 0;
        
        // move 2 back to move away from screen which is at z=1
        Matrix4x4 matrix = Matrix4x4.CreateFromYawPitchRoll(_yaw, _pitch, _roll) * Matrix4x4.CreateTranslation(new Vector3(0, 0, 2)); 
        for (int i = 0; i < vertices.Length; i++)
            vertices[i] = Vector3.Transform(vertices[i], matrix);


        foreach (var line in lineList)
        {
            DrawLine(_data, color,
                Transform(vertices[line.Item1]),
                Transform(vertices[line.Item2]));
        }

        // copy data to buffer
        Marshal.Copy(_data, 0, frameBuffer.Address, _data.Length);
        // dispose buffer to copy buffer to image
        frameBuffer.Dispose();
        // tell image to update visual
        Dispatcher.UIThread.Post(image.InvalidateVisual);
    }

    private void DrawLine(byte[] data, Color color, Vector2 start, Vector2 end)
    {
        Vector2 d = end - start;

        // calculate steps required for generating pixels 
        int steps = (int)(MathF.Abs(d.X) > MathF.Abs(d.Y) ? MathF.Abs(d.X) : MathF.Abs(d.Y));

        // calculate increment in x & y for each steps 
        float Xinc = d.X / steps;
        float Yinc = d.Y / steps;

        // Put pixel for each step 
        float x = start.X;
        float y = start.Y;

        for (int i = 0; i <= steps; i++, x += Xinc, y += Yinc)
            PutPixel(data, color, (int)MathF.Round(x), (int)MathF.Round(y));
    }

    private void PutPixel(byte[] data, Color color, int x, int y)
    {
        data[y * _width * 4 + x * 4] = color.R;
        data[y * _width * 4 + x * 4 + 1] = color.G;
        data[y * _width * 4 + x * 4 + 2] = color.B;
    }

    private Vector2 Transform(Vector3 vertex)
    {
        float distToScreen = 1;
        float inverse = distToScreen / vertex.Z;
        float xFactor = _width / 2;
        float yFactor = _height / 2;

        return new((vertex.X * inverse + 1) * xFactor, (-vertex.Y * inverse + 1) * yFactor);
    }

    private (LoadResult, (int, int)[]) Model3D()
    {
        var objLoaderFactory = new ObjLoaderFactory();
        var objLoader = objLoaderFactory.Create();
        var fileStream = AssetLoader.Open(new Uri("avares://AvaloniaRendering/Assets/cube.txt"));
        var result = objLoader.Load(fileStream);

        for (int i = result.Groups[0].Faces.Count - 1; i >= 0; i--)
        {
            var vertex0 = result.Vertices[result.Groups[0].Faces[i][0].VertexIndex - 1];
            var vertex1 = result.Vertices[result.Groups[0].Faces[i][1].VertexIndex - 1];
            var vertex2 = result.Vertices[result.Groups[0].Faces[i][2].VertexIndex - 1];

            Vector3 vec0 = new Vector3(vertex0.X, vertex0.Y, vertex0.Z);
            Vector3 vec1 = new Vector3(vertex1.X, vertex1.Y, vertex1.Z);
            Vector3 vec2 = new Vector3(vertex2.X, vertex2.Y, vertex2.Z);

            //Matrix4x4 matrix = Matrix4x4.CreateFromYawPitchRoll(0, -MathF.PI / 4, 0);
            //vec0 = Vector3.Transform(vec0, matrix);
            //vec1 = Vector3.Transform(vec1, matrix);
            //vec2 = Vector3.Transform(vec2, matrix);

            //matrix = Matrix4x4.CreateTranslation(new Vector3(0, 0, 2));
            //vec0 = Vector3.Transform(vec0, matrix);
            //vec1 = Vector3.Transform(vec1, matrix);
            //vec2 = Vector3.Transform(vec2, matrix);

        }
        (int, int)[] lineList = 
            [(0, 1), (1,2), (2,3), (3,0), // front
            (4,5), (5,6), (6,7), (7,4), // back
            (3,7), (2,6), (1,5), (0,4)]; // sides

        return (result, lineList);
    }

    private Vector3 VertexToVector(Vertex vertex) => new Vector3(vertex.X, vertex.Y, vertex.Z);

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        keyMap[e.Key] = true;
    }

    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        keyMap[e.Key] = false;
    }

}

