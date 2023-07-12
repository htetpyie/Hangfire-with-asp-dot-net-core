using HangfireDotNetCoreExample.Models;

namespace HangfireDotNetCoreExample.Features.Cron;

public class CronResponseModel
{
    public List<CronModel> RunningCronList { get; set; }
    public List<CronModel> StoppedCronList { get; set; }
}