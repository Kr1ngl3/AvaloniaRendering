using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaRendering.Engine;

record struct Face(
    int Vertex0,
    int Vertex1,
    int Vertex2,
    Vector2 TextureCoord0,
    Vector2 TextureCoord1,
    Vector2 TextureCoord2);
