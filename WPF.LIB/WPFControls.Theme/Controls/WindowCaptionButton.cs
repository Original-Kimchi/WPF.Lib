using System.Windows;
using System.Windows.Controls;

namespace WPFControls.Theme.Controls;

public enum WindowCaptionAction
{
    Minimize,
    MaximizeOrRestore,
    Close
}

public sealed class WindowCaptionButton : Button
{
    public static readonly DependencyProperty ActionProperty = DependencyProperty.Register(
        nameof(Action),
        typeof(WindowCaptionAction),
        typeof(WindowCaptionButton),
        new PropertyMetadata(WindowCaptionAction.Minimize));

    public WindowCaptionAction Action
    {
        get => (WindowCaptionAction)GetValue(ActionProperty);
        set => SetValue(ActionProperty, value);
    }

    protected override void OnClick()
    {
        base.OnClick();

        var window = Window.GetWindow(this);
        if (window is null)
        {
            return;
        }

        switch (Action)
        {
            case WindowCaptionAction.Minimize:
                SystemCommands.MinimizeWindow(window);
                break;
            case WindowCaptionAction.MaximizeOrRestore:
                if (window.WindowState == WindowState.Maximized)
                {
                    SystemCommands.RestoreWindow(window);
                }
                else
                {
                    SystemCommands.MaximizeWindow(window);
                }

                break;
            case WindowCaptionAction.Close:
                SystemCommands.CloseWindow(window);
                break;
        }
    }
}
