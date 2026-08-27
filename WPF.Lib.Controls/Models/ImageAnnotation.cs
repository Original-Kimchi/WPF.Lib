using System.Windows;
using System.Windows.Media;
using WPF.Lib.MVVM;

namespace WPF.Lib.Controls.Models;

public class ImageAnnotation : BaseModel
{
    private Point _start;
    private Point _end;
    private bool _isSelected;
    private AnnotationLabelVisibility _labelVisibility = AnnotationLabelVisibility.Always;
    private Color _objectColor = Color.FromRgb(255, 193, 7);
    private Brush _stroke;
    private Brush _fill;
    private double _strokeThickness = 2;
    private double _opacity = 1;
    private DoubleCollection? _strokeDashArray;
    private PenLineCap _strokeStartLineCap = PenLineCap.Round;
    private PenLineCap _strokeEndLineCap = PenLineCap.Round;
    private PenLineJoin _strokeLineJoin = PenLineJoin.Round;
    private Brush _labelBackground = CreateBrush(Color.FromArgb(204, 17, 24, 39));
    private Brush _labelForeground = Brushes.White;
    private double _labelOpacity = 1;

    public ImageAnnotation(ImageDrawingMode kind, Point start)
    {
        Kind = kind;
        _start = start;
        _end = start;
        _stroke = CreateBrush(_objectColor);
        _fill = CreateBrush(Color.FromArgb(32, _objectColor.R, _objectColor.G, _objectColor.B));
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
    public double CenterX => Left + (Width / 2);
    public double CenterY => Top + (Height / 2);
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

    public virtual Color ObjectColor
    {
        get => _objectColor;
        set
        {
            if (SetProperty(ref _objectColor, value))
            {
                Stroke = CreateBrush(value);
                Fill = CreateBrush(Color.FromArgb(32, value.R, value.G, value.B));
            }
        }
    }

    public virtual Brush Stroke
    {
        get => _stroke;
        set => SetProperty(ref _stroke, value ?? Brushes.Transparent);
    }

    public virtual Brush Fill
    {
        get => _fill;
        set => SetProperty(ref _fill, value ?? Brushes.Transparent);
    }

    public virtual double StrokeThickness
    {
        get => _strokeThickness;
        set => SetProperty(ref _strokeThickness, Math.Max(0, value));
    }

    public virtual double Opacity
    {
        get => _opacity;
        set => SetProperty(ref _opacity, Math.Clamp(value, 0, 1));
    }

    public virtual DoubleCollection? StrokeDashArray
    {
        get => _strokeDashArray;
        set => SetProperty(ref _strokeDashArray, value);
    }

    public virtual PenLineCap StrokeStartLineCap
    {
        get => _strokeStartLineCap;
        set => SetProperty(ref _strokeStartLineCap, value);
    }

    public virtual PenLineCap StrokeEndLineCap
    {
        get => _strokeEndLineCap;
        set => SetProperty(ref _strokeEndLineCap, value);
    }

    public virtual PenLineJoin StrokeLineJoin
    {
        get => _strokeLineJoin;
        set => SetProperty(ref _strokeLineJoin, value);
    }

    public virtual Brush LabelBackground
    {
        get => _labelBackground;
        set => SetProperty(ref _labelBackground, value ?? Brushes.Transparent);
    }

    public virtual Brush LabelForeground
    {
        get => _labelForeground;
        set => SetProperty(ref _labelForeground, value ?? Brushes.Transparent);
    }

    public virtual double LabelOpacity
    {
        get => _labelOpacity;
        set => SetProperty(ref _labelOpacity, Math.Clamp(value, 0, 1));
    }

    private void NotifyMeasurementsChanged()
    {
        OnPropertyChanged(nameof(Left));
        OnPropertyChanged(nameof(Top));
        OnPropertyChanged(nameof(Width));
        OnPropertyChanged(nameof(Height));
        OnPropertyChanged(nameof(CenterX));
        OnPropertyChanged(nameof(CenterY));
        OnPropertyChanged(nameof(Length));
    }

    private static Brush CreateBrush(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }
}
