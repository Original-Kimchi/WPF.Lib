using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WPF.Lib.MVVM;
using WPF.Lib.MVVM.Commands;

namespace WPF.Lib.Controls.Models;

public class ImageModel : BaseModel
{
    private ImageSource? _source;
    private double _scale = 1;
    private ImageDrawingMode _drawingMode = ImageDrawingMode.Pan;
    private ImageAnnotation? _selectedAnnotation;
    private double _unitsPerPixel = 1;
    private string _measurementUnit = "px";
    private bool _showLabels;
    private Rect _viewport = Rect.Empty;

    public ImageModel()
    {
        SetDrawingModeCommand = new RelayCommand(parameter =>
        {
            if (parameter is ImageDrawingMode mode)
            {
                DrawingMode = mode;
            }
            else if (Enum.TryParse(parameter?.ToString(), out ImageDrawingMode parsed))
            {
                DrawingMode = parsed;
            }
        });
        ResetScaleCommand = new RelayCommand(() => Scale = 1);
        DeleteSelectedCommand = new RelayCommand(
            DeleteSelected,
            () => SelectedAnnotation is not null);
        ClearAnnotationsCommand = new RelayCommand(Annotations.Clear);
    }

    public ImageSource? Source
    {
        get => _source;
        set
        {
            if (SetProperty(ref _source, value))
            {
                Viewport = Rect.Empty;
            }
        }
    }

    /// <summary>The visible image area in unscaled image coordinates, updated by ImageViewer.</summary>
    public Rect Viewport
    {
        get => _viewport;
        internal set => SetProperty(ref _viewport, value);
    }

    internal event EventHandler<Point>? NavigationRequested;

    /// <summary>Requests that the connected viewer center on a point in image coordinates.</summary>
    public void NavigateTo(Point imagePoint)
    {
        if (Source is not null && double.IsFinite(imagePoint.X) && double.IsFinite(imagePoint.Y))
        {
            NavigationRequested?.Invoke(this, imagePoint);
        }
    }

    public double Scale
    {
        get => _scale;
        set => SetProperty(ref _scale, Math.Clamp(value, 0.01, 10));
    }

    public ImageDrawingMode DrawingMode
    {
        get => _drawingMode;
        set => SetProperty(ref _drawingMode, value);
    }

    public ImageAnnotation? SelectedAnnotation
    {
        get => _selectedAnnotation;
        set
        {
            if (ReferenceEquals(_selectedAnnotation, value))
            {
                return;
            }

            if (_selectedAnnotation is not null)
            {
                _selectedAnnotation.IsSelected = false;
            }

            if (SetProperty(ref _selectedAnnotation, value))
            {
                if (_selectedAnnotation is not null)
                {
                    _selectedAnnotation.IsSelected = true;
                }

                DeleteSelectedCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public double UnitsPerPixel
    {
        get => _unitsPerPixel;
        set => SetProperty(ref _unitsPerPixel, value > 0 ? value : 1);
    }

    public string MeasurementUnit
    {
        get => _measurementUnit;
        set => SetProperty(ref _measurementUnit, string.IsNullOrWhiteSpace(value) ? "px" : value);
    }

    public bool ShowLabels
    {
        get => _showLabels;
        set => SetProperty(ref _showLabels, value);
    }

    public ObservableCollection<ImageAnnotation> Annotations { get; } = [];
    public ICommand SetDrawingModeCommand { get; }
    public ICommand ResetScaleCommand { get; }
    public RelayCommand DeleteSelectedCommand { get; }
    public ICommand ClearAnnotationsCommand { get; }

    public virtual ImageAnnotation CreateAnnotation(ImageDrawingMode kind, System.Windows.Point start)
    {
        return new ImageAnnotation(kind, start);
    }

    public void Clear()
    {
        SelectedAnnotation = null;
        Annotations.Clear();
        Source = null;
        Scale = 1;
        DrawingMode = ImageDrawingMode.Pan;
    }

    private void DeleteSelected()
    {
        if (SelectedAnnotation is not null)
        {
            Annotations.Remove(SelectedAnnotation);
            SelectedAnnotation = null;
        }
    }

}
