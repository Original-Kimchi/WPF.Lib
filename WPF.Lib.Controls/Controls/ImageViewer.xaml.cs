using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WPF.Lib.Controls.Models;

namespace WPF.Lib.Controls.Controls;

public partial class ImageViewer : UserControl
{
    public static IReadOnlyList<Color> ObjectColors { get; } =
    [
        Color.FromRgb(255, 193, 7),
        Color.FromRgb(239, 68, 68),
        Color.FromRgb(249, 115, 22),
        Color.FromRgb(34, 197, 94),
        Color.FromRgb(6, 182, 212),
        Color.FromRgb(59, 130, 246),
        Color.FromRgb(139, 92, 246),
        Color.FromRgb(236, 72, 153),
        Color.FromRgb(255, 255, 255)
    ];

    public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(
        nameof(Model), typeof(ImageModel), typeof(ImageViewer),
        new FrameworkPropertyMetadata(null, OnModelChanged));

    private Point _startPoint;
    private Point _panStart;
    private double _horizontalOffset;
    private double _verticalOffset;
    private ImageAnnotation? _draft;
    private ImageAnnotation? _editedAnnotation;
    private Point _originalStart;
    private Point _originalEnd;
    private string? _editOperation;
    private bool _isPanning;
    private bool _fitPending;

    public ImageViewer()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        SizeChanged += OnSizeChanged;
    }

    public ImageModel? Model
    {
        get => (ImageModel?)GetValue(ModelProperty);
        set => SetValue(ModelProperty, value);
    }

    private static void OnModelChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var viewer = (ImageViewer)sender;

        if (args.OldValue is ImageModel oldModel)
        {
            PropertyChangedEventManager.RemoveHandler(
                oldModel, viewer.OnModelPropertyChanged, nameof(ImageModel.Source));
        }

        if (args.NewValue is ImageModel newModel)
        {
            PropertyChangedEventManager.AddHandler(
                newModel, viewer.OnModelPropertyChanged, nameof(ImageModel.Source));
        }

        viewer.RequestFitToViewport();
        viewer.UpdateMiniMapViewport();
    }

    private void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ImageModel.Source))
        {
            RequestFitToViewport();
        }
    }

    private void RequestFitToViewport()
    {
        _fitPending = Model?.Source is not null;
        if (!_fitPending)
        {
            return;
        }

        Dispatcher.BeginInvoke(DispatcherPriority.Loaded, TryFitToViewport);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_fitPending)
        {
            TryFitToViewport();
        }
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (_fitPending)
        {
            TryFitToViewport();
        }
    }

    private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (Model is null || Keyboard.Modifiers != ModifierKeys.Control)
        {
            return;
        }

        Model.Scale *= e.Delta > 0 ? 1.15 : 1 / 1.15;
        e.Handled = true;
    }

    private void OnFitClick(object sender, RoutedEventArgs e)
    {
        FitToViewport();
    }

    public void FitToViewport()
    {
        _fitPending = true;
        TryFitToViewport();
    }

    private void TryFitToViewport()
    {
        if (Model?.Source is null ||
            Model.Source.Width <= 0 || Model.Source.Height <= 0 ||
            ImageScrollViewer.ViewportWidth <= 0 || ImageScrollViewer.ViewportHeight <= 0)
        {
            return;
        }

        const double margin = 2;
        var availableWidth = Math.Max(1, ImageScrollViewer.ViewportWidth - margin);
        var availableHeight = Math.Max(1, ImageScrollViewer.ViewportHeight - margin);
        var horizontalScale = availableWidth / Model.Source.Width;
        var verticalScale = availableHeight / Model.Source.Height;

        Model.Scale = Math.Min(horizontalScale, verticalScale);
        _fitPending = false;
        ImageScrollViewer.UpdateLayout();
        ImageScrollViewer.ScrollToHome();
        UpdateMiniMapViewport();
    }

    private void OnSurfaceMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (Model?.Source is null)
        {
            return;
        }

        Focus();
        _startPoint = Clamp(e.GetPosition(ImageSurface));
        if (Model.DrawingMode == ImageDrawingMode.Pan)
        {
            var element = e.OriginalSource as FrameworkElement;
            var annotation = FindAnnotation(element);
            if (annotation is not null)
            {
                Model.SelectedAnnotation = annotation;
                _editedAnnotation = annotation;
                _originalStart = annotation.Start;
                _originalEnd = annotation.End;
                _editOperation = element?.Tag as string ?? "Move";
            }
            else
            {
                Model.SelectedAnnotation = null;
                _isPanning = true;
                _panStart = e.GetPosition(ImageScrollViewer);
                _horizontalOffset = ImageScrollViewer.HorizontalOffset;
                _verticalOffset = ImageScrollViewer.VerticalOffset;
            }
        }
        else
        {
            _draft = Model.CreateAnnotation(Model.DrawingMode, _startPoint);
            Model.Annotations.Add(_draft);
            Model.SelectedAnnotation = _draft;
        }

        ImageSurface.CaptureMouse();
        e.Handled = true;
    }

    private void OnSurfaceMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        if (_draft is not null)
        {
            _draft.End = Clamp(e.GetPosition(ImageSurface));
        }
        else if (_editedAnnotation is not null)
        {
            EditAnnotation(Clamp(e.GetPosition(ImageSurface)));
        }
        else if (_isPanning)
        {
            var current = e.GetPosition(ImageScrollViewer);
            ImageScrollViewer.ScrollToHorizontalOffset(_horizontalOffset + _panStart.X - current.X);
            ImageScrollViewer.ScrollToVerticalOffset(_verticalOffset + _panStart.Y - current.Y);
        }
    }

    private void OnSurfaceMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_draft is not null)
        {
            _draft.End = Clamp(e.GetPosition(ImageSurface));
            if (_draft.Width < 1 && _draft.Height < 1)
            {
                Model?.Annotations.Remove(_draft);
            }
            else
            {
                NormalizeShape(_draft);
            }

            if (Model is not null)
            {
                Model.DrawingMode = ImageDrawingMode.Pan;
            }
        }

        if (_editedAnnotation is not null)
        {
            NormalizeShape(_editedAnnotation);
        }

        _draft = null;
        _editedAnnotation = null;
        _editOperation = null;
        _isPanning = false;
        ImageSurface.ReleaseMouseCapture();
        e.Handled = true;
    }

    private static ImageAnnotation? FindAnnotation(FrameworkElement? element)
    {
        while (element is not null)
        {
            if (element.DataContext is ImageAnnotation annotation)
            {
                return annotation;
            }

            element = element.Parent as FrameworkElement;
        }

        return null;
    }

    private void EditAnnotation(Point current)
    {
        if (_editedAnnotation is null)
        {
            return;
        }

        var constrainToAxis = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);
        switch (_editOperation)
        {
            case "ResizeStart":
                if (constrainToAxis)
                {
                    current = ConstrainToAxis(current, _originalEnd);
                }

                _editedAnnotation.Start = current;
                break;
            case "ResizeEnd":
                if (constrainToAxis)
                {
                    current = ConstrainToAxis(current, _originalStart);
                }

                _editedAnnotation.End = current;
                break;
            case "ResizeTopLeft":
                if (constrainToAxis)
                {
                    current = ConstrainToAxis(current, _originalStart);
                }

                _editedAnnotation.Start = current;
                break;
            case "ResizeTopRight":
                if (constrainToAxis)
                {
                    current = ConstrainToAxis(current, new Point(_originalEnd.X, _originalStart.Y));
                }

                _editedAnnotation.Start = new Point(_originalStart.X, current.Y);
                _editedAnnotation.End = new Point(current.X, _originalEnd.Y);
                break;
            case "ResizeBottomLeft":
                if (constrainToAxis)
                {
                    current = ConstrainToAxis(current, new Point(_originalStart.X, _originalEnd.Y));
                }

                _editedAnnotation.Start = new Point(current.X, _originalStart.Y);
                _editedAnnotation.End = new Point(_originalEnd.X, current.Y);
                break;
            case "ResizeBottomRight":
                if (constrainToAxis)
                {
                    current = ConstrainToAxis(current, _originalEnd);
                }

                _editedAnnotation.End = current;
                break;
            default:
                var delta = current - _startPoint;
                if (constrainToAxis)
                {
                    delta = ConstrainToAxis(delta);
                }

                var minX = Math.Min(_originalStart.X, _originalEnd.X);
                var maxX = Math.Max(_originalStart.X, _originalEnd.X);
                var minY = Math.Min(_originalStart.Y, _originalEnd.Y);
                var maxY = Math.Max(_originalStart.Y, _originalEnd.Y);
                delta.X = Math.Clamp(delta.X, -minX, ImageSurface.ActualWidth - maxX);
                delta.Y = Math.Clamp(delta.Y, -minY, ImageSurface.ActualHeight - maxY);
                _editedAnnotation.Start = _originalStart + delta;
                _editedAnnotation.End = _originalEnd + delta;
                break;
        }
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Delete || Model?.DeleteSelectedCommand.CanExecute(null) != true)
        {
            return;
        }

        Model.DeleteSelectedCommand.Execute(null);
        e.Handled = true;
    }

    private static Point ConstrainToAxis(Point current, Point origin)
    {
        var delta = current - origin;
        var constrainedDelta = ConstrainToAxis(delta);
        return origin + constrainedDelta;
    }

    private static Vector ConstrainToAxis(Vector delta)
    {
        return Math.Abs(delta.X) >= Math.Abs(delta.Y)
            ? new Vector(delta.X, 0)
            : new Vector(0, delta.Y);
    }

    private static void NormalizeShape(ImageAnnotation annotation)
    {
        if (annotation.Kind == ImageDrawingMode.Line)
        {
            return;
        }

        var start = new Point(annotation.Left, annotation.Top);
        var end = new Point(annotation.Left + annotation.Width, annotation.Top + annotation.Height);
        annotation.Start = start;
        annotation.End = end;
    }

    private Point Clamp(Point point)
    {
        return new Point(
            Math.Clamp(point.X, 0, ImageSurface.ActualWidth),
            Math.Clamp(point.Y, 0, ImageSurface.ActualHeight));
    }

    private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        UpdateMiniMapViewport();
    }

    private void UpdateMiniMapViewport()
    {
        var width = Math.Max(0, MiniMap.ActualWidth - 10);
        var height = Math.Max(0, MiniMap.ActualHeight - 10);
        var extentWidth = Math.Max(1, ImageScrollViewer.ExtentWidth);
        var extentHeight = Math.Max(1, ImageScrollViewer.ExtentHeight);

        MiniMapViewport.Width = Math.Min(width, width * ImageScrollViewer.ViewportWidth / extentWidth);
        MiniMapViewport.Height = Math.Min(height, height * ImageScrollViewer.ViewportHeight / extentHeight);
        MiniMapViewport.Margin = new Thickness(
            width * ImageScrollViewer.HorizontalOffset / extentWidth,
            height * ImageScrollViewer.VerticalOffset / extentHeight, 0, 0);
    }

    private void OnMiniMapMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var point = e.GetPosition(MiniMap);
        var xRatio = Math.Clamp(point.X / Math.Max(1, MiniMap.ActualWidth), 0, 1);
        var yRatio = Math.Clamp(point.Y / Math.Max(1, MiniMap.ActualHeight), 0, 1);
        ImageScrollViewer.ScrollToHorizontalOffset(
            xRatio * Math.Max(0, ImageScrollViewer.ExtentWidth - ImageScrollViewer.ViewportWidth));
        ImageScrollViewer.ScrollToVerticalOffset(
            yRatio * Math.Max(0, ImageScrollViewer.ExtentHeight - ImageScrollViewer.ViewportHeight));
        e.Handled = true;
    }
}
