using Avalonia;
using ObjLoader.Loader.Data.VertexData;
using SkiaSharp;
using Splat.ModeDetection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaRendering.Engine;

class Pipeline
{
    private readonly Graphics _graphics;
    private readonly Transformer _transformer;

    public Pipeline(Graphics graphics, Transformer transformer)
    {
        _graphics = graphics;
        _transformer = transformer;
    }

    public void Draw((Vector3[] Vertices, (int, int, int)[] Faces) model, Matrix4x4 transformMatrix)
    {
        ProcessVertices(model, transformMatrix);
    }

    private void ProcessVertices((Vector3[] Vertices, (int, int, int)[] Faces) model, Matrix4x4 transformMatrix)
    {
        Span<Vector3> vertices = stackalloc Vector3[model.Vertices.Length];
        model.Vertices.CopyTo(vertices);

        // transform from model space to view/world space
        for (int i = 0; i < vertices.Length; i++)
        {
            //vertices[i] = Vector3.Transform(vertices[i], Matrix4x4.CreateScale(new Vector3(1f/5)));
            vertices[i] = Vector3.Transform(vertices[i], transformMatrix);
        }

        AssembleTriangles(vertices, model.Faces);
    }

    private void AssembleTriangles(Span<Vector3> vertices, (int, int, int)[] faces)
    {
        for (int i = 0; i < faces.Length; i++)
        {
            Vector3 v0 = vertices[faces[i].Item1 - 1];
            Vector3 v1 = vertices[faces[i].Item2 - 1];
            Vector3 v2 = vertices[faces[i].Item3 - 1];


            // bigger than .5 for orto?
            if (Dot(Cross(v1 - v0, v2 - v0), v0) > 0)
                continue;

            ProcessTriangle(v0, v1, v2, i);
        }
    }

    // triangle processing function
    // takes 3 vertices to generate triangle
    // sends generated triangle to post-processing
    private void ProcessTriangle(Vector3 v0, Vector3 v1, Vector3 v2, int i)
    {
        // generate triangle from 3 vertices using gs
        // and send to post-processing
        PostProcessTriangleVertices(v0, v1, v2, i);
    }

    void PostProcessTriangleVertices(Vector3 v0, Vector3 v1, Vector3 v2, int i)
    {
        DrawTriangle(
            SKColor.FromHsv(i * 30 % 360, 100, 100),
            _transformer.Transform(v0),
            _transformer.Transform(v1),
            _transformer.Transform(v2));
    }

    //private Vector2 OrtoTransform(Vector3 vertex)
    //{
    //    Vector2 factor = new Vector2(_width / 2f, _height / 2f);

    //    return new Vector2((vertex.X + 1) * factor.X, (-vertex.Y + 1) * factor.Y);
    //}

    private void DrawTriangle(SKColor color, Vector2 v0, Vector2 v1, Vector2 v2)
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
                    _graphics.PutPixel(point, color);
                }
            }
        }
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
