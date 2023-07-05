namespace HangfireDotNetCoreExample.Features.DevCodes;

public static class CustomExtension
{
    public static bool IsNullOrEmpty(this string? str)
    {
        return str == null || string.IsNullOrEmpty(str);
    }
}