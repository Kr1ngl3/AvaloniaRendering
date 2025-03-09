
using Avalonia.Threading;
using AvaloniaRendering.Controls;
using SkiaSharp;
using System;
using System.Numerics;

namespace AvaloniaRendering.Engine;

class Graphics
{
    private readonly int _width;

    private readonly RenderingView _renderingView;
    private readonly SKBitmap _bitmap;

    public Graphics(RenderingView renderingView)
    {
        _width = (int)renderingView.Width;

        _renderingView = renderingView;
        _bitmap = renderingView.Bitmap;
    }

    /// <summary>
    /// Called before rendering
    /// </summary>
    public void BeginFrame()
    {
        _bitmap.Erase(SKColors.Black);
    }

    /// <summary>
    /// Called after rendering
    /// </summary>
    public void EndFrame()
    {
        Dispatcher.UIThread.InvokeAsync(_renderingView.InvalidateVisual);
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
        data[y * _width * 4 + x * 4] = color.Blue;
        data[y * _width * 4 + x * 4 + 1] = color.Green;
        data[y * _width * 4 + x * 4 + 2] = color.Red;
        data[y * _width * 4 + x * 4 + 3] = color.Alpha;
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
