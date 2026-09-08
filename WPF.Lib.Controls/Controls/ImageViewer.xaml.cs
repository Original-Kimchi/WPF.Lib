using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using WPF.Lib.Controls;
using WPF.Lib.Controls.Models;

namespace WPF.Lib.Controls.Controls;

public partial class ImageViewer : UserControl
{
    private const double SnapThresholdInScreenPixels = 8;

    #region Dependency Properties

    public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(
        nameof(Model), typeof(ImageModel), typeof(ImageViewer),
        new FrameworkPropertyMetadata(null, OnModelChanged));

    public static readonly DependencyProperty ShowMiniMapProperty = DependencyProperty.Register(
        nameof(ShowMiniMap), typeof(bool), typeof(ImageViewer), new PropertyMetadata(true));

    public bool ShowMiniMap
    {
        get => (bool)GetValue(ShowMiniMapProperty);
        set => SetValue(ShowMiniMapProperty, value);
    }

    #endregion

    #region Fields

    private Point _startPoint;
    private Point _panStart;
    private double _horizontalOffset;
    private double _verticalOffset;
    private ImageAnnotation? _draft;
    private ImageAnnotation? _editedAnnotation;
    private Point _originalStart;
    private Point _originalEnd;
    private ImageViewerEditOperation? _editOperation;
    private bool _isPanning;
    private bool _fitPending;
    private DispatcherOperation? _fitOperation;

    #endregion

    #region Model and Lifecycle

    public ImageViewer()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
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

        if (viewer.IsLoaded)
        {
            if (args.OldValue is ImageModel previousModel)
            {
                previousModel.NavigationRequested -= viewer.OnNavigationRequested;
            }

            if (args.NewValue is ImageModel currentModel)
            {
                currentModel.NavigationRequested += viewer.OnNavigationRequested;
            }
        }

        viewer.RequestFitToViewport();
        viewer.UpdateModelViewport();
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
        CancelPendingFit();
        _fitPending = Model?.Source is not null;
        if (!_fitPending || !IsLoaded)
        {
            return;
        }

        _fitOperation = Dispatcher.BeginInvoke(DispatcherPriority.Loaded, () =>
        {
            _fitOperation = null;
            TryFitToViewport();
        });
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (Model is not null)
        {
            Model.NavigationRequested += OnNavigationRequested;
        }

        UpdateModelViewport();
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

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (Model is not null)
        {
            Model.NavigationRequested -= OnNavigationRequested;
        }

        CancelPendingFit();
        _draft = null;
        _editedAnnotation = null;
        _editOperation = null;
        _isPanning = false;
        HideSnapGuides();

        if (ImageSurface.IsMouseCaptured)
        {
            ImageSurface.ReleaseMouseCapture();
        }
    }

    private void CancelPendingFit()
    {
        if (_fitOperation?.Status == DispatcherOperationStatus.Pending)
        {
            _fitOperation.Abort();
        }

        _fitOperation = null;
    }

    #endregion

    #region Zoom and Fit

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
        UpdateModelViewport();
    }

    #endregion

    #region Pointer Interaction

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
                _editOperation = element?.Tag is ImageViewerEditOperation operation
                    ? operation
                    : ImageViewerEditOperation.Move;
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

    #endregion

    #region Annotation Editing

    private void EditAnnotation(Point current)
    {
        if (_editedAnnotation is null)
        {
            return;
        }

        var constrainToAxis = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);
        if (_editOperation != ImageViewerEditOperation.Move)
        {
            var allowX = _editOperation is not ImageViewerEditOperation.ResizeTop and
                not ImageViewerEditOperation.ResizeBottom;
            var allowY = _editOperation is not ImageViewerEditOperation.ResizeLeft and
                not ImageViewerEditOperation.ResizeRight;

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
            case ImageViewerEditOperation.ResizeStart:
                _editedAnnotation.Start = current;
                break;
            case ImageViewerEditOperation.ResizeEnd:
                _editedAnnotation.End = current;
                break;
            case ImageViewerEditOperation.ResizeTopLeft:
                _editedAnnotation.Start = current;
                break;
            case ImageViewerEditOperation.ResizeTopRight:
                _editedAnnotation.Start = new Point(_originalStart.X, current.Y);
                _editedAnnotation.End = new Point(current.X, _originalEnd.Y);
                break;
            case ImageViewerEditOperation.ResizeBottomLeft:
                _editedAnnotation.Start = new Point(current.X, _originalStart.Y);
                _editedAnnotation.End = new Point(_originalEnd.X, current.Y);
                break;
            case ImageViewerEditOperation.ResizeBottomRight:
                _editedAnnotation.End = current;
                break;
            case ImageViewerEditOperation.ResizeTop:
                _editedAnnotation.Start = new Point(_originalStart.X, current.Y);
                break;
            case ImageViewerEditOperation.ResizeBottom:
                _editedAnnotation.End = new Point(_originalEnd.X, current.Y);
                break;
            case ImageViewerEditOperation.ResizeLeft:
                _editedAnnotation.Start = new Point(current.X, _originalStart.Y);
                break;
            case ImageViewerEditOperation.ResizeRight:
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
            ImageViewerEditOperation.ResizeStart => _originalEnd,
            ImageViewerEditOperation.ResizeEnd => _originalStart,
            ImageViewerEditOperation.ResizeTopRight => new Point(_originalEnd.X, _originalStart.Y),
            ImageViewerEditOperation.ResizeBottomLeft => new Point(_originalStart.X, _originalEnd.Y),
            ImageViewerEditOperation.ResizeBottomRight => _originalEnd,
            _ => _originalStart
        };
    }

    #endregion

    #region Keyboard Input

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

    #endregion

    #region Snapping

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
        AdditionalVerticalSnapGuide.StrokeThickness = thickness;
        HorizontalSnapGuide.StrokeThickness = thickness;
        AdditionalHorizontalSnapGuide.StrokeThickness = thickness;

        VerticalSnapGuide.Visibility = x.HasValue ? Visibility.Visible : Visibility.Collapsed;
        if (x.HasValue)
        {
            VerticalSnapGuide.X1 = x.Value;
            VerticalSnapGuide.X2 = x.Value;
            VerticalSnapGuide.Y1 = 0;
            VerticalSnapGuide.Y2 = ImageSurface.ActualHeight;
        }

        var additionalX = _editOperation == ImageViewerEditOperation.Move && x.HasValue
            ? FindAdditionalAlignedVerticalEdge(x.Value)
            : null;
        AdditionalVerticalSnapGuide.Visibility = additionalX.HasValue ? Visibility.Visible : Visibility.Collapsed;
        if (additionalX.HasValue)
        {
            AdditionalVerticalSnapGuide.X1 = additionalX.Value;
            AdditionalVerticalSnapGuide.X2 = additionalX.Value;
            AdditionalVerticalSnapGuide.Y1 = 0;
            AdditionalVerticalSnapGuide.Y2 = ImageSurface.ActualHeight;
        }

        HorizontalSnapGuide.Visibility = y.HasValue ? Visibility.Visible : Visibility.Collapsed;
        if (y.HasValue)
        {
            HorizontalSnapGuide.X1 = 0;
            HorizontalSnapGuide.X2 = ImageSurface.ActualWidth;
            HorizontalSnapGuide.Y1 = y.Value;
            HorizontalSnapGuide.Y2 = y.Value;
        }

        var additionalY = _editOperation == ImageViewerEditOperation.Move && y.HasValue
            ? FindAdditionalAlignedHorizontalEdge(y.Value)
            : null;
        AdditionalHorizontalSnapGuide.Visibility = additionalY.HasValue ? Visibility.Visible : Visibility.Collapsed;
        if (additionalY.HasValue)
        {
            AdditionalHorizontalSnapGuide.X1 = 0;
            AdditionalHorizontalSnapGuide.X2 = ImageSurface.ActualWidth;
            AdditionalHorizontalSnapGuide.Y1 = additionalY.Value;
            AdditionalHorizontalSnapGuide.Y2 = additionalY.Value;
        }
    }

    private double? FindAdditionalAlignedVerticalEdge(double primaryGuide)
    {
        if (Model is null || _editedAnnotation is null)
        {
            return null;
        }

        foreach (var target in Model.Annotations.Where(annotation => !ReferenceEquals(annotation, _editedAnnotation)))
        {
            if (!AreClose(_editedAnnotation.Left, primaryGuide) &&
                AreClose(_editedAnnotation.Left, target.Left))
            {
                return _editedAnnotation.Left;
            }

            var editedRight = _editedAnnotation.Left + _editedAnnotation.Width;
            var targetRight = target.Left + target.Width;
            if (!AreClose(editedRight, primaryGuide) && AreClose(editedRight, targetRight))
            {
                return editedRight;
            }
        }

        return null;
    }

    private double? FindAdditionalAlignedHorizontalEdge(double primaryGuide)
    {
        if (Model is null || _editedAnnotation is null)
        {
            return null;
        }

        foreach (var target in Model.Annotations.Where(annotation => !ReferenceEquals(annotation, _editedAnnotation)))
        {
            if (!AreClose(_editedAnnotation.Top, primaryGuide) &&
                AreClose(_editedAnnotation.Top, target.Top))
            {
                return _editedAnnotation.Top;
            }

            var editedBottom = _editedAnnotation.Top + _editedAnnotation.Height;
            var targetBottom = target.Top + target.Height;
            if (!AreClose(editedBottom, primaryGuide) && AreClose(editedBottom, targetBottom))
            {
                return editedBottom;
            }
        }

        return null;
    }

    private static bool AreClose(double first, double second)
    {
        return Math.Abs(first - second) < 0.001;
    }

    private void HideSnapGuides()
    {
        VerticalSnapGuide.Visibility = Visibility.Collapsed;
        AdditionalVerticalSnapGuide.Visibility = Visibility.Collapsed;
        HorizontalSnapGuide.Visibility = Visibility.Collapsed;
        AdditionalHorizontalSnapGuide.Visibility = Visibility.Collapsed;
    }

    #endregion

    #region Annotation Geometry

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

    #endregion

    #region Viewport Navigation

    private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        UpdateModelViewport();
    }

    private void UpdateModelViewport()
    {
        if (Model?.Source is not { } source)
        {
            return;
        }

        var scale = Model.Scale;
        var width = Math.Min(source.Width, ImageScrollViewer.ViewportWidth / scale);
        var height = Math.Min(source.Height, ImageScrollViewer.ViewportHeight / scale);
        Model.Viewport = new Rect(
            Math.Clamp(ImageScrollViewer.HorizontalOffset / scale, 0, Math.Max(0, source.Width - width)),
            Math.Clamp(ImageScrollViewer.VerticalOffset / scale, 0, Math.Max(0, source.Height - height)),
            Math.Max(0, width), Math.Max(0, height));
    }

    private void OnNavigationRequested(object? sender, Point imagePoint)
    {
        if (Model?.Source is null)
        {
            return;
        }

        ImageScrollViewer.ScrollToHorizontalOffset(
            imagePoint.X * Model.Scale - ImageScrollViewer.ViewportWidth / 2);
        ImageScrollViewer.ScrollToVerticalOffset(
            imagePoint.Y * Model.Scale - ImageScrollViewer.ViewportHeight / 2);
    }

    #endregion
}
