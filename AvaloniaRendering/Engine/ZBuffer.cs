using Avalonia.Controls.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaRendering.Engine;

class ZBuffer
{
    private readonly float[] _buffer;
    private readonly int _width;

    public ZBuffer(int width, int height)
    {
        _buffer = new float[width * height];
        _width = width;
    }

    public void Clear()
    {
        Array.Fill<float>(_buffer, float.MaxValue);
    }

    public bool TestAndSet(int x,  int y, float depth)
    {
        ref float currentDepth = ref _buffer[y * _width + x];
        if (depth < currentDepth)
        {
            currentDepth = depth;
            return true;
        }
        return false;
    }
}
