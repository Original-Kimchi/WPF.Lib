using System.Windows;
using System.Windows.Controls;
using WPF.Lib.Theme;

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

    public sealed record Member(string Initials, string Name, string Email, string Role);

    public sealed record Order(string Id, string Customer, string Date, string Status, string Total);
}
