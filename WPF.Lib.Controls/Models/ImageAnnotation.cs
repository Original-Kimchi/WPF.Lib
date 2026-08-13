using System.Windows;
using WPF.Lib.MVVM;

namespace WPF.Lib.Controls.Models;

public class ImageAnnotation : BaseModel
{
    private Point _start;
    private Point _end;
    private bool _isSelected;
    private AnnotationLabelVisibility _labelVisibility = AnnotationLabelVisibility.WhenSelected;

    public ImageAnnotation(ImageDrawingMode kind, Point start)
    {
        Kind = kind;
        _start = start;
        _end = start;
    }

    public ImageDrawingMode Kind { get; }

    public Point Start
    {
        get => _start;
        set
        {
            if (SetProperty(ref _start, value))
            {
                NotifyMeasurementsChanged();
            }
        }
    }

    public Point End
    {
        get => _end;
        set
        {
            if (SetProperty(ref _end, value))
            {
                NotifyMeasurementsChanged();
            }
        }
    }

    public double Left => Math.Min(Start.X, End.X);
    public double Top => Math.Min(Start.Y, End.Y);
    public double Width => Math.Abs(End.X - Start.X);
    public double Height => Math.Abs(End.Y - Start.Y);
    public double Length => Kind == ImageDrawingMode.Line
        ? Math.Sqrt(Math.Pow(End.X - Start.X, 2) + Math.Pow(End.Y - Start.Y, 2))
        : 0;

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (SetProperty(ref _isSelected, value))
            {
                OnPropertyChanged(nameof(IsLabelVisible));
            }
        }
    }

    public virtual AnnotationLabelVisibility LabelVisibility
    {
        get => _labelVisibility;
        set
        {
            if (SetProperty(ref _labelVisibility, value))
            {
                OnPropertyChanged(nameof(IsLabelVisible));
            }
        }
    }

    public virtual bool IsLabelVisible => LabelVisibility switch
    {
        AnnotationLabelVisibility.Always => true,
        AnnotationLabelVisibility.WhenSelected => IsSelected,
        _ => false
    };

    private void NotifyMeasurementsChanged()
    {
        OnPropertyChanged(nameof(Left));
        OnPropertyChanged(nameof(Top));
        OnPropertyChanged(nameof(Width));
        OnPropertyChanged(nameof(Height));
        OnPropertyChanged(nameof(Length));
    }
}
