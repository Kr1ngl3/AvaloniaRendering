
using Avalonia.Threading;
using AvaloniaRendering.Controls;
using SkiaSharp;
using System;
using System.Numerics;

namespace AvaloniaRendering.Engine;

class Graphics
{
    private readonly int _width;
    private readonly int _height;

    private readonly RenderingView _rendere;
    private readonly SKBitmap _bitmap;

    public Graphics(RenderingView rendere)
    {
        _width = (int)rendere.Width;
        _height = (int)rendere.Height;

        _rendere = rendere;
        _bitmap = rendere.Bitmap;
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
        Dispatcher.UIThread.InvokeAsync(_rendere.InvalidateVisual);
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

    public void DrawTriangle(SKColor color, Vector2 v0, Vector2 v1, Vector2 v2)
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
                    PutPixel(point, color);
                }
            }
        }
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

    /// <summary>
    /// Set pixel at point to given color
    /// Build in SetPixel looks slow
    /// </summary>
    /// <param name="point">Coordinates of pixel</param>
    /// <param name="color">Color to be set to</param>
    private void PutPixel(Vector2 point, SKColor color)
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
    private void PutPixel(int x, int y, SKColor color)
    {
        Span<byte> data = _bitmap.GetPixelSpan();
        data[y * _width * 4 + x * 4] = color.Blue;
        data[y * _width * 4 + x * 4 + 1] = color.Green;
        data[y * _width * 4 + x * 4 + 2] = color.Red;
        data[y * _width * 4 + x * 4 + 3] = color.Alpha;
    }
}
