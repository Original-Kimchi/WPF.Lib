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
    private const double SnapThresholdInScreenPixels = 8;

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

        HideSnapGuides();
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
        if (_editOperation != "Move")
        {
            var allowX = _editOperation is not "ResizeTop" and not "ResizeBottom";
            var allowY = _editOperation is not "ResizeLeft" and not "ResizeRight";

            if (constrainToAxis && allowX && allowY)
            {
                var constraintOrigin = GetResizeConstraintOrigin();
                current = ConstrainToAxis(current, constraintOrigin);
                var constrainedDelta = current - constraintOrigin;
                allowX = Math.Abs(constrainedDelta.X) >= Math.Abs(constrainedDelta.Y);
                allowY = !allowX;
            }

            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
            {
                current = SnapResizePoint(current, allowX, allowY, out var snapX, out var snapY);
                UpdateSnapGuides(snapX, snapY);
            }
            else
            {
                HideSnapGuides();
            }
        }

        switch (_editOperation)
        {
            case "ResizeStart":
                _editedAnnotation.Start = current;
                break;
            case "ResizeEnd":
                _editedAnnotation.End = current;
                break;
            case "ResizeTopLeft":
                _editedAnnotation.Start = current;
                break;
            case "ResizeTopRight":
                _editedAnnotation.Start = new Point(_originalStart.X, current.Y);
                _editedAnnotation.End = new Point(current.X, _originalEnd.Y);
                break;
            case "ResizeBottomLeft":
                _editedAnnotation.Start = new Point(current.X, _originalStart.Y);
                _editedAnnotation.End = new Point(_originalEnd.X, current.Y);
                break;
            case "ResizeBottomRight":
                _editedAnnotation.End = current;
                break;
            case "ResizeTop":
                _editedAnnotation.Start = new Point(_originalStart.X, current.Y);
                break;
            case "ResizeBottom":
                _editedAnnotation.End = new Point(_originalEnd.X, current.Y);
                break;
            case "ResizeLeft":
                _editedAnnotation.Start = new Point(current.X, _originalStart.Y);
                break;
            case "ResizeRight":
                _editedAnnotation.End = new Point(current.X, _originalEnd.Y);
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

                double? snapX = null;
                double? snapY = null;
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
                {
                    delta = SnapMoveDelta(delta, minX, maxX, minY, maxY, out snapX, out snapY);
                }

                _editedAnnotation.Start = _originalStart + delta;
                _editedAnnotation.End = _originalEnd + delta;
                UpdateSnapGuides(snapX, snapY);
                break;
        }
    }

    private Point GetResizeConstraintOrigin()
    {
        return _editOperation switch
        {
            "ResizeStart" => _originalEnd,
            "ResizeEnd" => _originalStart,
            "ResizeTopRight" => new Point(_originalEnd.X, _originalStart.Y),
            "ResizeBottomLeft" => new Point(_originalStart.X, _originalEnd.Y),
            "ResizeBottomRight" => _originalEnd,
            _ => _originalStart
        };
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Delete && Model?.DeleteSelectedCommand.CanExecute(null) == true)
        {
            Model.DeleteSelectedCommand.Execute(null);
            e.Handled = true;
            return;
        }

        if (!ReferenceEquals(Keyboard.FocusedElement, this) ||
            Model?.SelectedAnnotation is not ImageAnnotation annotation)
        {
            return;
        }

        var step = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ? 10d : 1d;
        var delta = e.Key switch
        {
            Key.Left => new Vector(-step, 0),
            Key.Right => new Vector(step, 0),
            Key.Up => new Vector(0, -step),
            Key.Down => new Vector(0, step),
            _ => default
        };

        if (delta == default)
        {
            return;
        }

        MoveAnnotation(annotation, delta);
        e.Handled = true;
    }

    private Vector SnapMoveDelta(
        Vector delta,
        double originalMinX,
        double originalMaxX,
        double originalMinY,
        double originalMaxY,
        out double? snapX,
        out double? snapY)
    {
        snapX = null;
        snapY = null;
        if (Model is null || _editedAnnotation is null)
        {
            return delta;
        }

        var threshold = SnapThresholdInScreenPixels / Math.Max(Model.Scale, 0.01);
        var movingX = new[]
        {
            originalMinX + delta.X,
            ((originalMinX + originalMaxX) / 2) + delta.X,
            originalMaxX + delta.X
        };
        var movingY = new[]
        {
            originalMinY + delta.Y,
            ((originalMinY + originalMaxY) / 2) + delta.Y,
            originalMaxY + delta.Y
        };

        var bestXDistance = threshold;
        var bestYDistance = threshold;
        var xAdjustment = 0d;
        var yAdjustment = 0d;

        var viewerX = new[] { 0d, ImageSurface.ActualWidth / 2, ImageSurface.ActualWidth };
        var viewerY = new[] { 0d, ImageSurface.ActualHeight / 2, ImageSurface.ActualHeight };
        FindClosestSnap(movingX, viewerX, ref bestXDistance, ref xAdjustment, ref snapX);
        FindClosestSnap(movingY, viewerY, ref bestYDistance, ref yAdjustment, ref snapY);

        foreach (var target in Model.Annotations.Where(annotation => !ReferenceEquals(annotation, _editedAnnotation)))
        {
            var targetX = new[] { target.Left, target.CenterX, target.Left + target.Width };
            var targetY = new[] { target.Top, target.CenterY, target.Top + target.Height };

            FindClosestSnap(movingX, targetX, ref bestXDistance, ref xAdjustment, ref snapX);
            FindClosestSnap(movingY, targetY, ref bestYDistance, ref yAdjustment, ref snapY);
        }

        var snappedX = Math.Clamp(
            delta.X + xAdjustment,
            -originalMinX,
            ImageSurface.ActualWidth - originalMaxX);
        var snappedY = Math.Clamp(
            delta.Y + yAdjustment,
            -originalMinY,
            ImageSurface.ActualHeight - originalMaxY);

        if (Math.Abs(snappedX - (delta.X + xAdjustment)) > double.Epsilon)
        {
            snapX = null;
        }

        if (Math.Abs(snappedY - (delta.Y + yAdjustment)) > double.Epsilon)
        {
            snapY = null;
        }

        return new Vector(snappedX, snappedY);
    }

    private Point SnapResizePoint(
        Point current,
        bool allowX,
        bool allowY,
        out double? snapX,
        out double? snapY)
    {
        snapX = null;
        snapY = null;
        if (Model is null || _editedAnnotation is null)
        {
            return current;
        }

        var threshold = SnapThresholdInScreenPixels / Math.Max(Model.Scale, 0.01);
        var bestXDistance = threshold;
        var bestYDistance = threshold;
        var xAdjustment = 0d;
        var yAdjustment = 0d;
        var movingX = new[] { current.X };
        var movingY = new[] { current.Y };

        var viewerX = new[] { 0d, ImageSurface.ActualWidth / 2, ImageSurface.ActualWidth };
        var viewerY = new[] { 0d, ImageSurface.ActualHeight / 2, ImageSurface.ActualHeight };
        if (allowX)
        {
            FindClosestSnap(movingX, viewerX, ref bestXDistance, ref xAdjustment, ref snapX);
        }

        if (allowY)
        {
            FindClosestSnap(movingY, viewerY, ref bestYDistance, ref yAdjustment, ref snapY);
        }

        foreach (var target in Model.Annotations.Where(annotation => !ReferenceEquals(annotation, _editedAnnotation)))
        {
            if (allowX)
            {
                var targetX = new[] { target.Left, target.CenterX, target.Left + target.Width };
                FindClosestSnap(movingX, targetX, ref bestXDistance, ref xAdjustment, ref snapX);
            }

            if (allowY)
            {
                var targetY = new[] { target.Top, target.CenterY, target.Top + target.Height };
                FindClosestSnap(movingY, targetY, ref bestYDistance, ref yAdjustment, ref snapY);
            }
        }

        return new Point(current.X + xAdjustment, current.Y + yAdjustment);
    }

    private static void FindClosestSnap(
        IEnumerable<double> movingAnchors,
        IEnumerable<double> targetAnchors,
        ref double bestDistance,
        ref double adjustment,
        ref double? guidePosition)
    {
        foreach (var movingAnchor in movingAnchors)
        {
            foreach (var targetAnchor in targetAnchors)
            {
                var difference = targetAnchor - movingAnchor;
                var distance = Math.Abs(difference);
                if (distance > bestDistance)
                {
                    continue;
                }

                bestDistance = distance;
                adjustment = difference;
                guidePosition = targetAnchor;
            }
        }
    }

    private void UpdateSnapGuides(double? x, double? y)
    {
        var thickness = 1 / Math.Max(Model?.Scale ?? 1, 0.01);
        VerticalSnapGuide.StrokeThickness = thickness;
        HorizontalSnapGuide.StrokeThickness = thickness;

        VerticalSnapGuide.Visibility = x.HasValue ? Visibility.Visible : Visibility.Collapsed;
        if (x.HasValue)
        {
            VerticalSnapGuide.X1 = x.Value;
            VerticalSnapGuide.X2 = x.Value;
            VerticalSnapGuide.Y1 = 0;
            VerticalSnapGuide.Y2 = ImageSurface.ActualHeight;
        }

        HorizontalSnapGuide.Visibility = y.HasValue ? Visibility.Visible : Visibility.Collapsed;
        if (y.HasValue)
        {
            HorizontalSnapGuide.X1 = 0;
            HorizontalSnapGuide.X2 = ImageSurface.ActualWidth;
            HorizontalSnapGuide.Y1 = y.Value;
            HorizontalSnapGuide.Y2 = y.Value;
        }
    }

    private void HideSnapGuides()
    {
        VerticalSnapGuide.Visibility = Visibility.Collapsed;
        HorizontalSnapGuide.Visibility = Visibility.Collapsed;
    }

    private void MoveAnnotation(ImageAnnotation annotation, Vector requestedDelta)
    {
        var minX = annotation.Left;
        var maxX = annotation.Left + annotation.Width;
        var minY = annotation.Top;
        var maxY = annotation.Top + annotation.Height;
        var delta = new Vector(
            Math.Clamp(requestedDelta.X, -minX, ImageSurface.ActualWidth - maxX),
            Math.Clamp(requestedDelta.Y, -minY, ImageSurface.ActualHeight - maxY));

        annotation.Start += delta;
        annotation.End += delta;
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
