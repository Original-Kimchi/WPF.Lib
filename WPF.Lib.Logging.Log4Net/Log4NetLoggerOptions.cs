namespace WPF.Lib.Logging.Log4Net;

public sealed class Log4NetLoggerOptions
{
    public string ConfigFilePath { get; set; } = "log4net.config";

    public string LogDirectory { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WPFControls.UI",
        "Logs");

    public string RepositoryName { get; set; } = "WPF.Lib.Logging.Log4Net";
}
