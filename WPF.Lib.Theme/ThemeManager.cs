using System.Windows;

namespace WPF.Lib.Theme;

public enum ThemeKind
{
    Light,
    Dark
}

public static class ThemeManager
{
    private const string ThemePathMarker = "/WPF.Lib.Theme;component/Theme/";

    public static void ApplyTheme(ThemeKind theme)
    {
        var dictionaries = Application.Current.Resources.MergedDictionaries;
        var currentTheme = dictionaries.FirstOrDefault(dictionary =>
            dictionary.Source?.OriginalString.Contains(
                ThemePathMarker,
                StringComparison.OrdinalIgnoreCase) == true);

        if (currentTheme is not null)
        {
            dictionaries.Remove(currentTheme);
        }

        dictionaries.Add(new ResourceDictionary
        {
            Source = new Uri(
                $"{ThemePathMarker}{theme}Theme.xaml",
                UriKind.Relative)
        });
    }
}
