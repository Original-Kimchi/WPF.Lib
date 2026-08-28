using System.Windows;
using WPF.Lib.Core.Abstractions;

namespace WPF.Lib.Services;

public sealed class AppLifetimeService : IAppLifetimeService
{
    public void Shutdown(int exitCode = 0)
    {
        Application.Current.Shutdown(exitCode);
    }
}
