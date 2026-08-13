using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WPF.Lib.MVVM.Dialogs;

namespace WPF.Lib.Theme.Controls;

public class BaseDialogView : Window
{
    private const string DialogStyleResourceKey = "BaseDialogViewStyle";
    private BaseDialogViewModel? _viewModel;
    private bool _closeRequested;

    public BaseDialogView()
    {
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        SetResourceReference(StyleProperty, DialogStyleResourceKey);
    }

    public DialogOutcome? Outcome { get; private set; }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        DataContextChanged += OnDataContextChanged;
        AttachViewModel(DataContext as BaseDialogViewModel);
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        if (e.ButtonState != MouseButtonState.Pressed ||
            e.GetPosition(this).Y > 44 ||
            IsInsideButton(e.OriginalSource as DependencyObject))
        {
            return;
        }

        DragMove();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!_closeRequested && DataContext is BaseDialogViewModel viewModel)
        {
            e.Cancel = true;
            viewModel.CancelCommand.Execute(null);
            return;
        }

        base.OnClosing(e);
    }

    protected override void OnClosed(EventArgs e)
    {
        DataContextChanged -= OnDataContextChanged;
        AttachViewModel(null);
        base.OnClosed(e);
    }

    private void OnDataContextChanged(
        object sender,
        DependencyPropertyChangedEventArgs e)
    {
        AttachViewModel(e.NewValue as BaseDialogViewModel);
    }

    private void AttachViewModel(BaseDialogViewModel? viewModel)
    {
        if (_viewModel is not null)
        {
            _viewModel.CloseRequested -= OnCloseRequested;
        }

        _viewModel = viewModel;

        if (_viewModel is not null)
        {
            _viewModel.CloseRequested += OnCloseRequested;
            SetCurrentValue(TitleProperty, _viewModel.Title);
        }
    }

    private void OnCloseRequested(
        object? sender,
        DialogCloseRequestedEventArgs e)
    {
        Outcome = e.Outcome;
        _closeRequested = true;

        try
        {
            DialogResult = e.Outcome == DialogOutcome.Confirmed;
        }
        catch (InvalidOperationException)
        {
            Close();
        }
    }

    private static bool IsInsideButton(DependencyObject? element)
    {
        while (element is not null)
        {
            if (element is Button)
            {
                return true;
            }

            element = VisualTreeHelper.GetParent(element);
        }

        return false;
    }
}
