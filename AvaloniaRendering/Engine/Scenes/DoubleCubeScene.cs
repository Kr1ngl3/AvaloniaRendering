using Avalonia;
using Avalonia.Input;
using AvaloniaRendering.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaRendering.Engine.Scenes;

class DoubleCubeScene : Scene
{
    const float MovePeriod = 2;

    private readonly (Vector3[] Vertices, Face[] Faces) _model;

    private float _z;

    public DoubleCubeScene(Graphics graphics, Transformer transformer) : base(graphics, transformer)
    {
        _model = Model3D();
    }

    public override void Update(RenderingView rendereingView, float deltaTime)
    {
        _z += rendereingView.KeyMap[Key.W] ? MovePeriod * deltaTime : 0;
        _z -= rendereingView.KeyMap[Key.S] ? MovePeriod * deltaTime : 0;
    }

    public override void Draw()
    {
        _pipeline.BeginFrame();
        
        Matrix4x4 matrix1 = Matrix4x4.CreateFromYawPitchRoll(MathF.PI / 4, MathF.PI / 4, 0) * Matrix4x4.CreateTranslation(new Vector3(0, 0, 3));
        Matrix4x4 matrix2 = Matrix4x4.CreateTranslation(new Vector3(0, 0, 3 + _z));

        _pipeline.Draw(_model, ref matrix1);

        _pipeline.Draw(_model, ref matrix2);
    }
}
