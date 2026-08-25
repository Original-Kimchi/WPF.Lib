namespace WPF.Lib.Api;

public sealed class ApiOptions
{
    public Uri BaseAddress { get; set; } = new("http://localhost:5000/");

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
}
