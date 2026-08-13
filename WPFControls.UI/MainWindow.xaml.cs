using System.Windows;
using WPFControls.UI.ViewModels;

namespace WPFControls.UI;

public partial class MainWindow : Window
{
    public MainWindow(ShellViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
