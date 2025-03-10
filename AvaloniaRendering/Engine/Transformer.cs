using AvaloniaRendering.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaRendering.Engine;

class Transformer
{
    private readonly int _width;
    private readonly int _height;

    public Transformer(int width, int height)
    {
        _width = width;
        _height = height;
    }

    public void Transform(ref Vertex vertex)
    {
        float distToScreen = 1;
        float inverse = distToScreen / vertex.Position.Z;
        Vector2 factor = new Vector2(_width / 2f, _height / 2f);

        vertex.Position = new Vector3((vertex.Position.X * inverse + 1) * factor.X, (-vertex.Position.Y * inverse + 1) * factor.Y, vertex.Position.Z);
    }
}
