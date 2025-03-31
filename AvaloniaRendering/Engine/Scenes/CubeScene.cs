using Avalonia.Input;
using Avalonia.Platform;
using AvaloniaRendering.Controls;
using ObjLoader.Loader.Data.VertexData;
using ObjLoader.Loader.Loaders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaRendering.Engine.Scenes;

class CubeScene : Scene
{
    const float RotatePeriod = MathF.PI;

    private float _yaw;
    private float _pitch;
    private float _roll;

    private readonly (Vector3[] Vertices, Face[] Faces) _model;

    public CubeScene(Graphics graphics, Transformer transformer) : base(graphics, transformer)
    {
        _model = Model3D();
    }

    public override void Update(RenderingView rendereingView, float deltaTime)
    {
        _yaw += rendereingView.KeyMap[Key.A] ? RotatePeriod * deltaTime : 0;
        _yaw -= rendereingView.KeyMap[Key.D] ? RotatePeriod * deltaTime : 0;

        _pitch += rendereingView.KeyMap[Key.W] ? RotatePeriod * deltaTime : 0;
        _pitch -= rendereingView.KeyMap[Key.S] ? RotatePeriod * deltaTime : 0;

        _roll += rendereingView.KeyMap[Key.Q] ? RotatePeriod * deltaTime : 0;
        _roll -= rendereingView.KeyMap[Key.E] ? RotatePeriod * deltaTime : 0;

    }

    public override void Draw()
    {
        _pipeline.BeginFrame();

        // move 2 back to move away from screen which is at z=1
        Matrix4x4 matrix = Matrix4x4.CreateFromYawPitchRoll(_yaw, _pitch, _roll) * Matrix4x4.CreateTranslation(new Vector3(0, 0, 2));

        _pipeline.Draw(_model, ref matrix);
    }
}
