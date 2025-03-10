using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaRendering.Engine.Shaders;

class TexturePS : PixelShader
{
    private readonly SKBitmap _texture;

    private readonly int _textureWidth;
    private readonly int _textureHeight;



    public TexturePS(SKBitmap texture)
    {
        _texture = texture;

        _textureWidth = _texture.Width;
        _textureHeight = _texture.Height;
    }

    public override SKColor Shade(Vector3 fragCoord, Vector2 texCoord)
    {
        //perform texture lookup, clamp, and write pixel
        return _texture.GetPixel(
            (int)MathF.Min(texCoord.X * _textureWidth + 0.5f, _textureWidth - 1),
            (int)MathF.Min(texCoord.Y * _textureHeight + 0.5f, _textureHeight - 1)
        );
    }
}