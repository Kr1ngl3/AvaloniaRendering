
using Avalonia.Threading;
using AvaloniaRendering.Controls;
using SkiaSharp;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace AvaloniaRendering.Engine;

class Graphics
{
    const int BytesPerPixel = 4;

    private readonly RenderingView _renderingView;
    private readonly SKBitmap _bitmap;

    public int Width { get; }
    public int Height { get; }

    public Graphics(RenderingView renderingView)
    {
        Width = (int)renderingView.Width;
        Height = (int)renderingView.Height;

        _renderingView = renderingView;
        _bitmap = renderingView.Bitmap;
    }

    /// <summary>
    /// Called before rendering
    /// </summary>
    public void BeginFrame()
    {
        //for (int y = 0; y < Height; y++)
        //{
        //    for (int x = 0; x < Width; x++)
        //    {
        //        PutPixel(x, y, SKColors.Black);
        //    }
        //}
        Parallel.For(0, Width * Height, i =>
        {
            int y = i / Width;     // Integer division to get the row (y-coordinate)
            int x = i % Width;     // Modulo to get the column (x-coordinate)

            PutPixel(x, y, SKColors.Black);
        });
    }

    /// <summary>
    /// Called after rendering
    /// </summary>
    public void EndFrame()
    {
        Dispatcher.UIThread.Invoke(_renderingView.InvalidateVisual);
        NOP(0.008);
    }
    private static void NOP(double durationSeconds)
    {
        var durationTicks = Math.Round(durationSeconds * Stopwatch.Frequency);
        var sw = Stopwatch.StartNew();

        while (sw.ElapsedTicks < durationTicks)
        {

        }
    }

    /// <summary>
    /// Set pixel at point to given color
    /// Build in SetPixel looks slow
    /// </summary>
    /// <param name="point">Coordinates of pixel</param>
    /// <param name="color">Color to be set to</param>
    public void PutPixel(Vector2 point, SKColor color)
    {
        PutPixel((int)MathF.Round(point.X), (int)MathF.Round(point.Y), color);
    }

    /// <summary>
    /// Set pixel at point to given color
    /// Build in SetPixel looks slow
    /// </summary>
    /// <param name="x">X coordinate of pixel</param>
    /// <param name="y">Y coordinate of pixel</param>
    /// <param name="color">Color to be set to</param>
    public void PutPixel(int x, int y, SKColor color)
    {
        Span<byte> data = _bitmap.GetPixelSpan();
        data[y * Width * BytesPerPixel + x * BytesPerPixel] = color.Blue;
        data[y * Width * BytesPerPixel + x * BytesPerPixel + 1] = color.Green;
        data[y * Width * BytesPerPixel + x * BytesPerPixel + 2] = color.Red;
        data[y * Width * BytesPerPixel + x * BytesPerPixel + 3] = color.Alpha;
    }

    /// <summary>
    /// Draws line to bitmap
    /// Source: https://www.geeksforgeeks.org/dda-line-generation-algorithm-computer-graphics/
    /// </summary>
    /// <param name="color">Color of line</param>
    /// <param name="start">Start point</param>
    /// <param name="end">End point</param>
    private void DrawLine(Vector2 start, Vector2 end, SKColor color)
    {
        Vector2 d = end - start;

        // calculate steps required for generating pixels 
        int steps = (int)MathF.Round((MathF.Abs(d.X) > MathF.Abs(d.Y) ? MathF.Abs(d.X) : MathF.Abs(d.Y)));

        // calculate increment in x & y for each steps 
        Vector2 inc = d / steps;

        // Put pixel for each step 
        Vector2 point = new Vector2(start.X, start.Y);
        for (int i = 0; i <= steps; i++, point.X += inc.X, point.Y += inc.Y)
            PutPixel(point, color);
    }

}
