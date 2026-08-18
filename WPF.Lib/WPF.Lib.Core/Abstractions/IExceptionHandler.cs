namespace WPF.Lib.Core.Abstractions;

public interface IExceptionHandler
{
    void Handle(Exception exception, string source, bool isTerminating = false);
}
