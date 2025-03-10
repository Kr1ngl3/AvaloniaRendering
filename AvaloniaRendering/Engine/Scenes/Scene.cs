using AvaloniaRendering.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaRendering.Engine.Scenes;

abstract class Scene
{
    protected Pipeline _pipeline;

    protected Scene(Graphics graphics, Transformer transformer)
    {
        _pipeline = new Pipeline(graphics, transformer);
    }

    public abstract void Update(RenderingView rendereingView, float deltaTime);
    public abstract void Draw();
}
