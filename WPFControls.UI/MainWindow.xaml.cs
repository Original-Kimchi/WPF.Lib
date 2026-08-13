using System.Windows;
using System.Windows.Controls;
using WPF.Lib.MVVM.Dialogs;
using WPF.Lib.Theme;
using WPFControls.UI.Dialogs;
using WPFControls.UI.ViewModels;

namespace WPFControls.UI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
    }

    public IReadOnlyList<Member> Members { get; } =
    [
        new("AK", "Alex Kim", "alex@example.com", "Designer"),
        new("JL", "Jamie Lee", "jamie@example.com", "Developer"),
        new("SP", "Sam Park", "sam@example.com", "Product Manager"),
        new("MC", "Morgan Choi", "morgan@example.com", "QA Engineer")
    ];

    public IReadOnlyList<Order> Orders { get; } =
    [
        new("#1048", "Acme Studio", "2026-08-12", "Completed", "$1,240"),
        new("#1047", "Northwind", "2026-08-11", "Processing", "$860"),
        new("#1046", "Contoso", "2026-08-10", "Completed", "$2,150"),
        new("#1045", "Fabrikam", "2026-08-09", "Pending", "$540"),
        new("#1044", "Adventure Works", "2026-08-08", "Completed", "$975")
    ];

    private void ThemeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ThemeSelector.SelectedItem is not ComboBoxItem { Tag: string themeName } ||
            !Enum.TryParse(themeName, out ThemeKind theme))
        {
            return;
        }

        ThemeManager.ApplyTheme(theme);
    }

    private void ShowDialog_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = new ConfirmationDialogViewModel(
            "변경 사항 확인",
            "선택한 설정을 적용합니다. 계속 진행하려면 확인을 눌러주세요.");

        var dialog = new ConfirmationDialog
        {
            Owner = this,
            DataContext = viewModel
        };

        dialog.ShowDialog();

        DialogResultText.Text = dialog.Outcome switch
        {
            DialogOutcome.Confirmed => "결과: 확인",
            DialogOutcome.Cancelled => "결과: 취소 또는 닫기",
            _ => "결과: 없음"
        };
    }

    public sealed record Member(string Initials, string Name, string Email, string Role);

    public sealed record Order(string Id, string Customer, string Date, string Status, string Total);
}
