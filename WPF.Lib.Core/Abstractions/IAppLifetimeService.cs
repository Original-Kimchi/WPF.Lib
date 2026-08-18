namespace WPF.Lib.Core.Abstractions;

public interface IAppLifetimeService
{
    void Shutdown(int exitCode = 0);
}
