using HangfireDotNetCoreExample.Models;

namespace HangfireDotNetCoreExample.Features.Cron;

public class CronResponseModel
{
    public List<CronModel> CronList { get; set; }
    public int RunningCrons { get; set; }
    public int StoppedCrons { get; set; }
}