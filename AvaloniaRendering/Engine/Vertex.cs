using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaRendering.Engine;

record struct Vertex(Vector3 Position, Vector2 TextureCoord)
{
    public static Vertex operator+ (Vertex lhs, Vertex rhs)
    {
        return new Vertex(lhs.Position + rhs.Position, lhs.TextureCoord + rhs.TextureCoord);
    }

    public static Vertex operator -(Vertex lhs, Vertex rhs)
    {
        return new Vertex(lhs.Position - rhs.Position, lhs.TextureCoord - rhs.TextureCoord);
    }

    public static Vertex operator /(Vertex lhs, float rhs)
    {
        return new Vertex(lhs.Position / rhs, lhs.TextureCoord / rhs);
    }

    public static Vertex operator *(Vertex lhs, float rhs)
    {
        return new Vertex(lhs.Position * rhs, lhs.TextureCoord * rhs);
    }
}