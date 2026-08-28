namespace WPF.Lib.Api;

public sealed class ApiOptions
{
    public const long DefaultMaximumResponseContentBytes = 10 * 1024 * 1024;

    public Uri BaseAddress { get; set; } = new("http://localhost:5000/");

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    public long MaximumResponseContentBytes { get; set; } = DefaultMaximumResponseContentBytes;
}
