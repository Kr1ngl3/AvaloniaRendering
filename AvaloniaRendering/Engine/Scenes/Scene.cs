using Avalonia.Platform;
using AvaloniaRendering.Controls;
using ObjLoader.Loader.Data.VertexData;
using ObjLoader.Loader.Loaders;
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

    protected (Vector3[] Vertices, Face[] Faces) Model3D()
    {
        var objLoaderFactory = new ObjLoaderFactory();
        var objLoader = objLoaderFactory.Create();
        using var fileStream = AssetLoader.Open(new Uri("avares://AvaloniaRendering/Assets/cube.txt"));
        var result = objLoader.Load(fileStream);

        return (
            result.Vertices
                .Select(VertexToVector)
                .ToArray(),
            result.Groups[0].Faces
                .Select(face => new Face(
                    face[0].VertexIndex,
                    face[1].VertexIndex,
                    face[2].VertexIndex,
                    TextureToVector(result.Textures[face[0].TextureIndex - 1]),
                    TextureToVector(result.Textures[face[1].TextureIndex - 1]),
                    TextureToVector(result.Textures[face[2].TextureIndex - 1])
                ))
                .ToArray());
    }

    protected Vector3 VertexToVector(ObjLoader.Loader.Data.VertexData.Vertex vertex) => new Vector3(vertex.X, vertex.Y, vertex.Z);
    protected Vector2 TextureToVector(Texture texture) => new Vector2(texture.X, texture.Y);
}
