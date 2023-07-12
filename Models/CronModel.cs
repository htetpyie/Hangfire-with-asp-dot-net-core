using LiteDB;

namespace HangfireDotNetCoreExample.Models;

public class CronModel
{
    [BsonId]
    public int Id { get; set; }
    public string JobId { get; set; }
    public string Name { get; set; }
    public string LastTime { get; set; }
    public string NextTime { get; set; }
    public string StoppedTime { get; set; }
    public bool IsRunning { get; set; }

    public string Status
    {
        get => IsRunning ? "Running" : "Stopped";
    }
    public string StopUrl
    {
        get => IsRunning ? $"/Blog/StopCron?jobId={JobId}" : "#";
    }
}