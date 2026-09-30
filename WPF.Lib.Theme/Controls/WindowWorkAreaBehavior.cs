using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace WPF.Lib.Theme.Controls;

/// <summary>
/// Keeps custom-chrome maximized windows inside the monitor work area.
/// </summary>
public static class WindowWorkAreaBehavior
{
    private const int WmGetMinMaxInfo = 0x0024;
    private const uint MonitorDefaultToNearest = 0x00000002;

    public static readonly DependencyProperty ConstrainMaximizedWindowProperty =
        DependencyProperty.RegisterAttached(
            "ConstrainMaximizedWindow",
            typeof(bool),
            typeof(WindowWorkAreaBehavior),
            new PropertyMetadata(false, OnConstrainMaximizedWindowChanged));

    public static void SetConstrainMaximizedWindow(DependencyObject element, bool value) =>
        element.SetValue(ConstrainMaximizedWindowProperty, value);

    public static bool GetConstrainMaximizedWindow(DependencyObject element) =>
        (bool)element.GetValue(ConstrainMaximizedWindowProperty);

    private static void OnConstrainMaximizedWindowChanged(
        DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs eventArgs)
    {
        if (dependencyObject is not Window window)
        {
            return;
        }

        if ((bool)eventArgs.NewValue)
        {
            window.SourceInitialized += OnWindowSourceInitialized;
        }
        else
        {
            window.SourceInitialized -= OnWindowSourceInitialized;
            RemoveHook(window);
        }
    }

    private static void OnWindowSourceInitialized(object? sender, EventArgs eventArgs)
    {
        if (sender is not Window window)
        {
            return;
        }

        window.SourceInitialized -= OnWindowSourceInitialized;

        if (PresentationSource.FromVisual(window) is HwndSource source)
        {
            source.AddHook(WindowProcedure);
            window.Closed += OnWindowClosed;
        }
    }

    private static void OnWindowClosed(object? sender, EventArgs eventArgs)
    {
        if (sender is Window window)
        {
            window.Closed -= OnWindowClosed;
            RemoveHook(window);
        }
    }

    private static void RemoveHook(Window window)
    {
        if (PresentationSource.FromVisual(window) is HwndSource source)
        {
            source.RemoveHook(WindowProcedure);
        }
    }

    private static IntPtr WindowProcedure(
        IntPtr windowHandle,
        int message,
        IntPtr wordParameter,
        IntPtr longParameter,
        ref bool handled)
    {
        if (message != WmGetMinMaxInfo)
        {
            return IntPtr.Zero;
        }

        var monitor = MonitorFromWindow(windowHandle, MonitorDefaultToNearest);
        var monitorInfo = new MonitorInfo { Size = Marshal.SizeOf<MonitorInfo>() };

        if (monitor == IntPtr.Zero || !GetMonitorInfo(monitor, ref monitorInfo))
        {
            return IntPtr.Zero;
        }

        var minMaxInfo = Marshal.PtrToStructure<MinMaxInfo>(longParameter);
        minMaxInfo.MaxPosition.X = monitorInfo.WorkArea.Left - monitorInfo.MonitorArea.Left;
        minMaxInfo.MaxPosition.Y = monitorInfo.WorkArea.Top - monitorInfo.MonitorArea.Top;
        minMaxInfo.MaxSize.X = monitorInfo.WorkArea.Right - monitorInfo.WorkArea.Left;
        minMaxInfo.MaxSize.Y = monitorInfo.WorkArea.Bottom - monitorInfo.WorkArea.Top;
        Marshal.StructureToPtr(minMaxInfo, longParameter, true);

        handled = true;
        return IntPtr.Zero;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr windowHandle, uint flags);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetMonitorInfo(IntPtr monitorHandle, ref MonitorInfo monitorInfo);

    [StructLayout(LayoutKind.Sequential)]
    private struct Point
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MinMaxInfo
    {
        public Point Reserved;
        public Point MaxSize;
        public Point MaxPosition;
        public Point MinTrackSize;
        public Point MaxTrackSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Rectangle
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct MonitorInfo
    {
        public int Size;
        public Rectangle MonitorArea;
        public Rectangle WorkArea;
        public uint Flags;
    }
}
