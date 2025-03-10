using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaRendering.Engine.Shaders;

class PixelShader
{
    public virtual SKColor Shade(Vertex vertex)
    {
        return new SKColor((byte)(vertex.TextureCoord.X * 255), (byte)(vertex.TextureCoord.Y * 255), 0, 255);
    }
}
