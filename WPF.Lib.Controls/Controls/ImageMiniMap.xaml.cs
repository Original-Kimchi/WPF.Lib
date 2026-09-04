using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WPF.Lib.Controls.Models;

namespace WPF.Lib.Controls.Controls;

/// <summary>Displays and navigates the viewport of an ImageViewer sharing the same ImageModel.</summary>
public partial class ImageMiniMap : UserControl
{
    public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(
        nameof(Model), typeof(ImageModel), typeof(ImageMiniMap),
        new FrameworkPropertyMetadata(null, OnModelChanged));

    public ImageMiniMap()
    {
        InitializeComponent();
        Loaded += (_, _) => UpdateViewport();
        Unloaded += (_, _) => MapSurface.ReleaseMouseCapture();
    }

    public ImageModel? Model
    {
        get => (ImageModel?)GetValue(ModelProperty);
        set => SetValue(ModelProperty, value);
    }

    private static void OnModelChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var map = (ImageMiniMap)sender;
        map.MapSurface.ReleaseMouseCapture();
        if (args.OldValue is ImageModel oldModel)
        {
            PropertyChangedEventManager.RemoveHandler(oldModel, map.OnModelPropertyChanged, string.Empty);
        }

        if (args.NewValue is ImageModel newModel)
        {
            PropertyChangedEventManager.AddHandler(newModel, map.OnModelPropertyChanged, string.Empty);
        }

        map.UpdateViewport();
    }

    private void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ImageModel.Source) or nameof(ImageModel.Viewport) or null or "")
        {
            UpdateViewport();
        }
    }

    private Rect GetImageBounds()
    {
        if (Model?.Source is not { } source || source.Width <= 0 || source.Height <= 0 ||
            MapSurface.ActualWidth <= 0 || MapSurface.ActualHeight <= 0)
        {
            return Rect.Empty;
        }

        var scale = Math.Min(MapSurface.ActualWidth / source.Width, MapSurface.ActualHeight / source.Height);
        var width = source.Width * scale;
        var height = source.Height * scale;
        return new Rect((MapSurface.ActualWidth - width) / 2, (MapSurface.ActualHeight - height) / 2, width, height);
    }

    private void OnMapSizeChanged(object sender, SizeChangedEventArgs e) => UpdateViewport();

    private void UpdateViewport()
    {
        var bounds = GetImageBounds();
        var viewport = Model?.Viewport ?? Rect.Empty;
        if (bounds.IsEmpty || viewport.IsEmpty)
        {
            ViewportRectangle.Visibility = Visibility.Collapsed;
            return;
        }

        viewport.Intersect(new Rect(0, 0, Model!.Source!.Width, Model.Source.Height));
        if (viewport.IsEmpty)
        {
            ViewportRectangle.Visibility = Visibility.Collapsed;
            return;
        }

        var scale = bounds.Width / Model.Source.Width;
        Canvas.SetLeft(ViewportRectangle, bounds.Left + viewport.Left * scale);
        Canvas.SetTop(ViewportRectangle, bounds.Top + viewport.Top * scale);
        ViewportRectangle.Width = viewport.Width * scale;
        ViewportRectangle.Height = viewport.Height * scale;
        ViewportRectangle.Visibility = Visibility.Visible;
    }

    private void OnMapMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var bounds = GetImageBounds();
        if (bounds.IsEmpty || !bounds.Contains(e.GetPosition(MapSurface)))
        {
            return;
        }

        MapSurface.CaptureMouse();
        Navigate(e.GetPosition(MapSurface));
        e.Handled = true;
    }

    private void OnMapMouseMove(object sender, MouseEventArgs e)
    {
        if (MapSurface.IsMouseCaptured && e.LeftButton == MouseButtonState.Pressed)
        {
            Navigate(e.GetPosition(MapSurface));
            e.Handled = true;
        }
    }

    private void OnMapMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (MapSurface.IsMouseCaptured)
        {
            MapSurface.ReleaseMouseCapture();
            e.Handled = true;
        }
    }

    private void Navigate(Point point)
    {
        var bounds = GetImageBounds();
        if (bounds.IsEmpty)
        {
            return;
        }

        Model!.NavigateTo(new Point(
            Math.Clamp((point.X - bounds.Left) / bounds.Width, 0, 1) * Model.Source!.Width,
            Math.Clamp((point.Y - bounds.Top) / bounds.Height, 0, 1) * Model.Source.Height));
    }
}
