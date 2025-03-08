using Avalonia;
using Avalonia.Platform;
using ObjLoader.Loader.Loaders;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace AvaloniaRendering.ViewModels;

public class MainViewModel : ViewModelBase
{
    public string Greeting => "Welcome to Avalonia!";

    public static double Profile(string description, int iterations, Action func)
    {
        //Run at highest priority to minimize fluctuations caused by other processes/threads
        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
        Thread.CurrentThread.Priority = ThreadPriority.Highest;

        // warm up 
        func();

        var watch = new Stopwatch();

        // clean up
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        watch.Start();
        for (int i = 0; i < iterations; i++)
        {
            func();
        }
        watch.Stop();
        Trace.Write(description);
        Trace.WriteLine($" Time Elapsed {watch.Elapsed.TotalMilliseconds} ms");
        return watch.Elapsed.TotalMilliseconds;
    }
}