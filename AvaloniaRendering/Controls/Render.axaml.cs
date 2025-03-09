using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using ObjLoader.Loader.Loaders;
using System.Numerics;
using System;
using ObjLoader.Loader.Data.VertexData;
using System.Linq;
using System.Collections.Generic;
using Avalonia.Input;
using System.Timers;
using Avalonia.Threading;
using SkiaSharp;
using Avalonia.Skia;

using Avalonia.Rendering.SceneGraph;
using Avalonia.Controls.Documents;
using System.Diagnostics;
using Avalonia.Animation;
using Avalonia.Media.Imaging;
using AvaloniaRendering.Engine;

namespace AvaloniaRendering.Controls;

public partial class RenderingView : UserControl
{
    private static readonly Key[] UsedKeys = { Key.W, Key.S, Key.Q, Key.E, Key.A, Key.D, Key.Escape};

    private readonly Game _game;

    public Dictionary<Key, bool> KeyMap { get; } = new();

    public SKBitmap Bitmap { get; private set; }

    public RenderingView()
    {
        InitializeComponent();

        Bitmap = new SKBitmap((int)Width, (int)Height);


        foreach (Key key in UsedKeys)
            KeyMap[key] = false;

        KeyDownEvent.AddClassHandler<TopLevel>(OnKeyDown, handledEventsToo: true);
        KeyUpEvent.AddClassHandler<TopLevel>(OnKeyUp, handledEventsToo: true);

        _game = new Game(this);
        _game.Start();
    }

    public void End()
    {
        Bitmap.Dispose();

        Bitmap = null!;

        Dispatcher.UIThread.Post(() => Content = new TextBlock
        {
            Foreground = Brushes.Black,
            Text = "Games done"
        });
        
    }

    public override void Render(DrawingContext context)
    {
        if (Bitmap is null)
            return;

        context.Custom(new ImageDrawOperation(Bitmap, (int)Width, (int)Height));
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        KeyMap[e.Key] = true;
    }

    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        KeyMap[e.Key] = false;
    }

    private class ImageDrawOperation : ICustomDrawOperation
    {
        private readonly SKBitmap _bitmap;
        public ImageDrawOperation(SKBitmap bitmap, int width, int height)
        {
            _bitmap = bitmap;
            Bounds = new Rect(0, 0, width, height);
        }

        public Rect Bounds { get; }
        public void Dispose() { }
        public bool Equals(ICustomDrawOperation? other) => false;
        public bool HitTest(Point p) => false;

        public void Render(ImmediateDrawingContext context)
        {
            // Get canvas to draw on
            var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();

            if (leaseFeature is null)
                throw new Exception($"Could not get {nameof(ISkiaSharpApiLeaseFeature)} from {nameof(ImmediateDrawingContext)}");

            using var lease = leaseFeature.Lease();
            var canvas = lease.SkCanvas;

            // draw bitmap
            canvas.DrawBitmap(_bitmap, new SKPoint(0, 0));
        }
    }
}