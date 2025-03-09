using Avalonia;
using Avalonia.Controls.Platform;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Platform;
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
    private readonly SKBitmap _texture;

    public Pipeline(Graphics graphics, Transformer transformer)
    {
        _texture = new SKBitmap(10, 10);
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                _texture.SetPixel(i, j, SKColor.FromHsv(10 * i * j % 360, 100, 100));
            }
        }
        using var fileStream = AssetLoader.Open(new Uri("avares://AvaloniaRendering/Assets/obama.png"));

        _texture = SKBitmap.Decode(fileStream);

        _graphics = graphics;
        _transformer = transformer;
    }

    public void Draw((Vector3[] Vertices, Face[] Faces) model, Matrix4x4 transformMatrix)
    {
        ProcessVertices(model, transformMatrix);
    }

    private void ProcessVertices((Vector3[] Vertices, Face[] Faces) model, Matrix4x4 transformMatrix)
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

    private void AssembleTriangles(Span<Vector3> vertices, Face[] faces)
    {
        for (int i = 0; i < faces.Length; i++)
        {
            Vertex v0 = new Vertex(vertices[faces[i].Vertex0 - 1], faces[i].TextureCoord0);
            Vertex v1 = new Vertex(vertices[faces[i].Vertex1 - 1], faces[i].TextureCoord1);
            Vertex v2 = new Vertex(vertices[faces[i].Vertex2 - 1], faces[i].TextureCoord2);


            // bigger than .5 for orto?
            if (Dot(Cross(v1.Position - v0.Position, v2.Position - v0.Position), v0.Position) > 0)
                continue;

            ProcessTriangle(v0, v1, v2);
        }
    }

    // triangle processing function
    // takes 3 vertices to generate triangle
    // sends generated triangle to post-processing
    private void ProcessTriangle(Vertex v0, Vertex v1, Vertex v2)
    {
        // generate triangle from 3 vertices using gs
        // and send to post-processing
        PostProcessTriangleVertices(v0, v1, v2);
    }

    private void PostProcessTriangleVertices(Vertex v0, Vertex v1, Vertex v2)
    {

        v0.Position = _transformer.Transform(v0.Position);
        v1.Position = _transformer.Transform(v1.Position);
        v2.Position = _transformer.Transform(v2.Position);

        DrawTriangle(v0, v1, v2);
    }

    private void DrawTriangle(Vertex v0, Vertex v1, Vertex v2)
	{
		// sorting vertices by y
		if (v1.Position.Y < v0.Position.Y) (v0, v1) = (v1, v0);
		if (v2.Position.Y < v1.Position.Y) (v1, v2) = (v2, v1);
		if (v1.Position.Y < v0.Position.Y) (v0, v1) = (v1, v0);

		if (v0.Position.Y == v1.Position.Y) // natural flat top
		{
			// sorting top vertices by x
			if (v1.Position.X < v0.Position.X) (v0, v1) = (v1, v0);

			DrawFlatTopTriangle(v0, v1, v2);
}
		else if(v1.Position.Y == v2.Position.Y) // natural flat bottom
		{
			// sorting bottom vertices by x
			if(v2.Position.X < v1.Position.X) (v1, v2) = (v2, v1);

			DrawFlatBottomTriangle(v0, v1, v2);
		}

        else // general triangle
        {
            // find splitting vertex interpolant
            float alphaSplit =
                (v1.Position.Y - v0.Position.Y) /
                (v2.Position.Y - v0.Position.Y);

            Vertex vi = Interpolate(v0, v2, alphaSplit);

            if (v1.Position.X < vi.Position.X) // major right
            {
                DrawFlatBottomTriangle(v0, v1, vi);
                DrawFlatTopTriangle(v1, vi, v2);
            }
            else // major left
            {
                DrawFlatBottomTriangle(v0, vi, v1);
                DrawFlatTopTriangle(vi, v1, v2);
            }
        }
    }

    // does flat *TOP* tri-specific calculations and calls DrawFlatTriangle
    private void DrawFlatTopTriangle(Vertex v0, Vertex v1, Vertex v2)
    {
        // calulcate dVertex / dy
        // change in interpolant for every 1 change in y
        float delta_y = v2.Position.Y - v0.Position.Y;
        Vertex dit0 = (v2 - v0) / delta_y;
        Vertex dit1 = (v2 - v1) / delta_y;

        // call the flat triangle render routine
        DrawFlatTriangle(v0, v1, v2, dit0, dit1, v1);
    }

    // does flat *BOTTOM* tri-specific calculations and calls DrawFlatTriangle
    private void DrawFlatBottomTriangle(Vertex v0, Vertex v1, Vertex v2)
    {
        // calulcate dVertex / dy
        // change in interpolant for every 1 change in y
        float delta_y = v2.Position.Y - v0.Position.Y;
        Vertex dit0 = (v1 - v0) / delta_y;
        Vertex dit1 = (v2 - v0) / delta_y;


        // call the flat triangle render routine
        DrawFlatTriangle(v0, v1, v2, dit0, dit1, v0 );
	}

    // does processing common to both flat top and flat bottom tris
    // texture lookup and pixel written here
    void DrawFlatTriangle(
        Vertex v0,
	    Vertex v1,
	    Vertex v2,
	    Vertex dv0,
	    Vertex dv1,
	    Vertex edge1)
	{
        // create edge interpolant for left edge (always v0)
        Vertex edge0 = v0;

        // calculate start and end scanlines
        int yStart = (int)MathF.Ceiling(v0.Position.Y - 0.5f);
        int yEnd = (int)MathF.Ceiling(v2.Position.Y - 0.5f); // the scanline AFTER the last line drawn

        // do interpolant prestep
        edge0 += dv0 * ((float)yStart + 0.5f - v0.Position.Y);
		edge1 += dv1 * ((float)yStart + 0.5f - v0.Position.Y);

		// prepare clamping constants
        float tex_xclamp = _texture.Width - 1.0f;
        float tex_yclamp = _texture.Height - 1.0f;

		for(int y = yStart; y < yEnd; y++, edge0 += dv0, edge1 += dv1)
		{
			// calculate start and end pixels
			int xStart = (int)MathF.Ceiling(edge0.Position.X - 0.5f);
            int xEnd = (int)MathF.Ceiling(edge1.Position.X - 0.5f); // the pixel AFTER the last pixel drawn

            // create scanline interpolant startpoint
            // (some waste for interpolating x,y,z, but makes life easier not having
            //  to split them off, and z will be needed in the future anyways...)
            Vertex iLine = edge0;

            // calculate delta scanline interpolant / dx
            float dx = edge1.Position.X - edge0.Position.X;
            Vertex diLine = (edge1 - iLine) / dx;

            // prestep scanline interpolant
            iLine += diLine * ((float)xStart + 0.5f - edge0.Position.X);

			for(int x = xStart; x < xEnd; x++, iLine += diLine )
			{
				// perform texture lookup, clamp, and write pixel
				_graphics.PutPixel(x, y, _texture.GetPixel(
                    (int)MathF.Min(iLine.TextureCoord.X * _texture.Width + 0.5f, tex_xclamp ),
					(int)MathF.Min(iLine.TextureCoord.Y * _texture.Height + 0.5f, tex_yclamp )
				));
			}
		}
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

    Vertex Interpolate(Vertex source, Vertex destination, float alpha )
    {
	    return source + (destination - source) * alpha;
    }
}
