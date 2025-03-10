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
    public virtual SKColor Shade(Vector3 fragCoord, Vector2 texCoord)
    {
        return new SKColor((byte)(texCoord.X * 255), (byte)(texCoord.Y * 255), 0, 255);
    }
}
