using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using ObjLoader.Loader.Loaders;
using System.Numerics;
using System;
using ObjLoader.Loader.Data.VertexData;
using System.Linq;
using System.Collections.Generic;
using Avalonia.Input;
using System.Timers;
using Avalonia.Threading;
using SkiaSharp;
using Avalonia.Skia;

using Avalonia.Rendering.SceneGraph;
using Avalonia.Controls.Documents;
using System.Diagnostics;
using Avalonia.Animation;

namespace AvaloniaRendering.Controls;
public partial class Rendere : UserControl
{
    const int FPS = 60;
    const float RotateSpeed = MathF.PI / FPS;
    private static readonly Key[] UsedKeys = { Key.W, Key.S, Key.Q, Key.E, Key.A, Key.D };

    private readonly int _width;
    private readonly int _height;
    private readonly SKBitmap _bitmap;
    private readonly Dictionary<Key, bool> keyMap = new();

    private float _yaw;
    private float _pitch;
    private float _roll;


    public Rendere()
    {
        InitializeComponent();

        _width = (int)Width;
        _height = (int)Height;
        _bitmap = new SKBitmap(_width, _height);

        foreach (Key key in UsedKeys)
            keyMap[key] = false;

        KeyDownEvent.AddClassHandler<TopLevel>(OnKeyDown, handledEventsToo: true);
        KeyUpEvent.AddClassHandler<TopLevel>(OnKeyUp, handledEventsToo: true);

        Initialize();
    }

    static DateTime last = DateTime.Now;
    private void Initialize()
    {
        var model = Model3D();

        Timer timer = new Timer(1d / FPS * 1000);
        timer.Elapsed += (sender, eventArgs) =>
        {
            Span<Vector3> vertices = stackalloc Vector3[model.Vertices.Length];
            model.Vertices.CopyTo(vertices);
            Compose(_bitmap.GetPixelSpan(), vertices, model.Faces, eventArgs.SignalTime.TimeOfDay.TotalMilliseconds / 10 % 360);
        };
        timer.Start();
        
    }

    public override void Render(DrawingContext context)
    {
        context.Custom(new ImageDrawOperation(_bitmap, _width, _height));
    }


    static int counter = 0;
    private void Compose(Span<byte> data, Span<Vector3> vertices, (int, int, int)[] faces, double timeHue)
    {
        Color background = HsvColor.ToRgb(0, 1, 0);

        Clear(data, background);

        _yaw += keyMap[Key.A] ? RotateSpeed : 0;
        _yaw -= keyMap[Key.D] ? RotateSpeed : 0;

        _pitch += keyMap[Key.W] ? RotateSpeed : 0;
        _pitch -= keyMap[Key.S] ? RotateSpeed : 0;

        _roll += keyMap[Key.Q] ? RotateSpeed : 0;
        _roll -= keyMap[Key.E] ? RotateSpeed : 0;

        // move 2 back to move away from screen which is at z=1
        Matrix4x4 matrix = Matrix4x4.CreateFromYawPitchRoll(_yaw, _pitch, _roll) * Matrix4x4.CreateTranslation(new Vector3(0, 0, 2));

        // transform from model space to view/world space
        for (int i = 0; i < vertices.Length; i++)
        {
            //vertices[i] = Vector3.Transform(vertices[i], Matrix4x4.CreateScale(new Vector3(1f/5)));
            vertices[i] = Vector3.Transform(vertices[i], matrix);
        }

        for (int i = 0; i < faces.Length; i++)
        {
            Vector3 v0 = vertices[faces[i].Item1 - 1];
            Vector3 v1 = vertices[faces[i].Item2 - 1];
            Vector3 v2 = vertices[faces[i].Item3 - 1];

            if (Dot(Cross(v1 - v0, v2 - v0), v0) >= 0)
                continue;

            DrawTriangle(data, 
                HsvColor.ToRgb(i * 30 + timeHue % 360, 1, 1), 
                Transform(v0), 
                Transform(v1), 
                Transform(v2));
        }

        Dispatcher.UIThread.InvokeAsync(InvalidateVisual);
    }
    /// <summary>
    /// Source: https://github.com/SebLague/Gamedev-Maths/blob/master/PointInTriangle.cs
    /// </summary>
    /// <param name="v0">First vertex of triangle</param>
    /// <param name="v1">Second vertex of triangle</param>
    /// <param name="v2">Third vertex of triangle</param>
    /// <param name="point">Point to look at</param>
    /// <returns>Whether the point is inside the triangle</returns>
    private bool IsInsideTriangle(Vector2 v0, Vector2 v1, Vector2 v2, Vector2 point)
    {
        double s1 = v2.Y - v0.Y;
        double s2 = v2.X - v0.X;
        double s3 = v1.Y - v0.Y;
        double s4 = point.Y - v0.Y;

        // fix bug from deriviation of equation
        s1 = s1 == 0 ? 1 : s1;

        double w1 = (v0.X * s1 + s4 * s2 - point.X * s1) / (s3 * s2 - (v1.X - v0.X) * s1);
        double w2 = (s4 - w1 * s3) / s1;
        return w1 >= 0 && w2 >= 0 && (w1 + w2) <= 1;
    }

    private void DrawTriangle(Span<byte> data, Color color, Vector2 v0, Vector2 v1, Vector2 v2)
    {
        // Find the bounding box of the triangle
        int minX = (int)MathF.Round(MathF.Min(MathF.Min(v0.X, v1.X), v2.X));
        int maxX = (int)MathF.Round(MathF.Max(MathF.Max(v0.X, v1.X), v2.X));
        int minY = (int)MathF.Round(MathF.Min(MathF.Min(v0.Y, v1.Y), v2.Y));
        int maxY = (int)MathF.Round(MathF.Max(MathF.Max(v0.Y, v1.Y), v2.Y));

        Vector2 point;

        // Iterate over each pixel in the bounding box
        for (point.Y = minY; point.Y <= maxY; point.Y++)
        {
            for (point.X = minX; point.X <= maxX; point.X++)
            {
                // If the point is inside the triangle, plot it
                if (IsInsideTriangle(v0, v1, v2, point))
                {
                    PutPixel(data, color, point);
                }
            }
        }
    }

    /// <summary>
    /// Source: https://www.geeksforgeeks.org/dda-line-generation-algorithm-computer-graphics/
    /// </summary>
    /// <param name="data">Span to write to</param>
    /// <param name="color">Color of line</param>
    /// <param name="start">Start point</param>
    /// <param name="end">End point</param>
    private void DrawLine(Span<byte> data, Color color, Vector2 start, Vector2 end)
    {
        Vector2 d = end - start;

        // calculate steps required for generating pixels 
        int steps = (int)MathF.Round((MathF.Abs(d.X) > MathF.Abs(d.Y) ? MathF.Abs(d.X) : MathF.Abs(d.Y)));

        // calculate increment in x & y for each steps 
        float Xinc = d.X / steps;
        float Yinc = d.Y / steps;

        // Put pixel for each step 
        float x = start.X;
        float y = start.Y;

        for (int i = 0; i <= steps; i++, x += Xinc, y += Yinc)
            PutPixel(data, color, (int)MathF.Round(x), (int)MathF.Round(y));
    }

    private void PutPixel(Span<byte> data, Color color, Vector2 point)
    {
        PutPixel(data, color, (int)MathF.Round(point.X), (int)MathF.Round(point.Y));
    }

    private void PutPixel(Span<byte> data, Color color, int x, int y)
    {
        data[y * _width * 4 + x * 4] = color.B;
        data[y * _width * 4 + x * 4 + 1] = color.G;
        data[y * _width * 4 + x * 4 + 2] = color.R;
        data[y * _width * 4 + x * 4 + 3] = color.A;
    }

    private void Clear(Span<byte> data)
    {
        Clear(data, Colors.Black);
    }

    private void Clear(Span<byte> data, Color color)
    {
        for (int i = 0; i < data.Length; i += 4)
        {
            PutPixel(data, color, i / _width, i % _width);
        }
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

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        keyMap[e.Key] = true;
        if (e.Key == Key.Escape)
            _bitmap.Dispose();
    }

    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        keyMap[e.Key] = false;
    }

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

class ImageDrawOperation : ICustomDrawOperation
{
    private readonly SKBitmap _bitmap;
    public ImageDrawOperation(SKBitmap bitmap, int width, int height)
    {
        _bitmap = bitmap;
        Bounds = new Rect(0, 0, width, height);
    }

    public Rect Bounds { get; }
    public void Dispose() { }
    public bool Equals(ICustomDrawOperation? other) => false;
    public bool HitTest(Point p) => false;

    public void Render(ImmediateDrawingContext context)
    {
        // Get canvas to draw on
        var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();

        if (leaseFeature is null)
            throw new Exception($"Could not get {nameof(ISkiaSharpApiLeaseFeature)} from {nameof(ImmediateDrawingContext)}");

        using var lease = leaseFeature.Lease();
        var canvas = lease.SkCanvas;

        // draw bitmap
        canvas.DrawBitmap(_bitmap, new SKPoint(0,0));
    }
}