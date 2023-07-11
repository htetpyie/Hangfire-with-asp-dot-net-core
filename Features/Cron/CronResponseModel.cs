using HangfireDotNetCoreExample.Models;

namespace HangfireDotNetCoreExample.Features.Cron;

public class CronResponseModel
{
    public List<CronModel> CronList { get; set; }
}