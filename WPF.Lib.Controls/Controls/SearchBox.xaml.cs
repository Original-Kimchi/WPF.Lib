using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace WPF.Lib.Controls.Controls;

public partial class SearchBox : UserControl
{
    private readonly DispatcherTimer _searchTimer;

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text), typeof(string), typeof(SearchBox),
        new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(
        nameof(Placeholder), typeof(string), typeof(SearchBox), new PropertyMetadata("Search"));

    public static readonly DependencyProperty SearchCommandProperty = DependencyProperty.Register(
        nameof(SearchCommand), typeof(ICommand), typeof(SearchBox));

    public static readonly DependencyProperty SearchCommandParameterProperty = DependencyProperty.Register(
        nameof(SearchCommandParameter), typeof(object), typeof(SearchBox));

    public static readonly DependencyProperty SearchOnTextChangedProperty = DependencyProperty.Register(
        nameof(SearchOnTextChanged), typeof(bool), typeof(SearchBox), new PropertyMetadata(false));

    public static readonly DependencyProperty SearchDelayProperty = DependencyProperty.Register(
        nameof(SearchDelay), typeof(int), typeof(SearchBox), new PropertyMetadata(350, OnSearchDelayChanged, CoerceSearchDelay));

    public SearchBox()
    {
        InitializeComponent();
        _searchTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(SearchDelay) };
        _searchTimer.Tick += OnSearchTimerTick;
        Unloaded += OnUnloaded;
        GotKeyboardFocus += (_, _) => OuterBorder.SetResourceReference(Border.BorderBrushProperty, "PrimaryBrush");
        LostKeyboardFocus += (_, _) => OuterBorder.SetResourceReference(Border.BorderBrushProperty, "BorderBrush");
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public ICommand? SearchCommand
    {
        get => (ICommand?)GetValue(SearchCommandProperty);
        set => SetValue(SearchCommandProperty, value);
    }

    public object? SearchCommandParameter
    {
        get => GetValue(SearchCommandParameterProperty);
        set => SetValue(SearchCommandParameterProperty, value);
    }

    public bool SearchOnTextChanged
    {
        get => (bool)GetValue(SearchOnTextChangedProperty);
        set => SetValue(SearchOnTextChangedProperty, value);
    }

    public int SearchDelay
    {
        get => (int)GetValue(SearchDelayProperty);
        set => SetValue(SearchDelayProperty, value);
    }

    private void ExecuteSearch()
    {
        var parameter = SearchCommandParameter ?? Text;
        if (SearchCommand?.CanExecute(parameter) == true)
        {
            SearchCommand.Execute(parameter);
        }
    }

    private void OnInputPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
        {
            return;
        }

        _searchTimer.Stop();
        ExecuteSearch();
        e.Handled = true;
    }

    private void OnInputTextChanged(object sender, TextChangedEventArgs e)
    {
        if (!SearchOnTextChanged || !IsLoaded)
        {
            return;
        }

        _searchTimer.Stop();
        _searchTimer.Start();
    }

    private void OnClearClick(object sender, RoutedEventArgs e)
    {
        Text = string.Empty;
        InputTextBox.Focus();
        if (SearchOnTextChanged && IsLoaded)
        {
            _searchTimer.Stop();
            _searchTimer.Start();
        }
    }

    private void OnSearchTimerTick(object? sender, EventArgs e)
    {
        _searchTimer.Stop();
        ExecuteSearch();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _searchTimer.Stop();
    }

    private static object CoerceSearchDelay(DependencyObject d, object baseValue) => Math.Max(0, (int)baseValue);

    private static void OnSearchDelayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (SearchBox)d;
        if (control._searchTimer is not null)
        {
            control._searchTimer.Interval = TimeSpan.FromMilliseconds((int)e.NewValue);
        }
    }
}
