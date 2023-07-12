using HangfireDotNetCoreExample.DbService;
using HangfireDotNetCoreExample.Models;

namespace HangfireDotNetCoreExample.Features.Cron;

public class CronTaskService
{
    private readonly CronService _cronService;
    private readonly LiteDbService _liteDbService;

    public CronTaskService(
        CronService cronService,
        LiteDbService liteDbService)
    {
        _cronService = cronService;
        _liteDbService = liteDbService;
    }

    public void SaveTask(CronModel cron)
    {
        _liteDbService.Insert(cron);
    }

    public List<CronModel> GetAllStoppedTask()
    {
        return _liteDbService.GetList<CronModel>();
    }
}