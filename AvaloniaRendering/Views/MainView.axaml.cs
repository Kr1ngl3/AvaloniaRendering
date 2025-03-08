using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Platform;
using ObjLoader.Loader.Data.VertexData;
using ObjLoader.Loader.Loaders;
using System;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Numerics;

namespace AvaloniaRendering.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }
}
