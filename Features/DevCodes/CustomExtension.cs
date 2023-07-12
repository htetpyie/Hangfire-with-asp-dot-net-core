namespace HangfireDotNetCoreExample.Features.DevCodes;

public static class CustomExtension
{
    public static bool IsNullOrEmpty(this string? str)
    {
        return str == null || string.IsNullOrEmpty(str);
    }

    public static string GetDateTimeString(this DateTime dateTime)
    {
        if (dateTime == null) return "";
        return TimeZoneInfo
            .ConvertTimeBySystemTimeZoneId(
                dateTime,
                "Myanmar Standard Time")
            .ToString("f");
    }
}