namespace WPF.Lib.Api;

public sealed class ApiResponseTooLargeException : Exception
{
    public ApiResponseTooLargeException(long maximumBytes)
        : base($"API response content exceeded the configured limit of {maximumBytes} bytes.")
    {
        MaximumBytes = maximumBytes;
    }

    public long MaximumBytes { get; }
}
