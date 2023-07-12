using HangfireDotNetCoreExample.DbService;
using HangfireDotNetCoreExample.Models;

namespace HangfireDotNetCoreExample.Features.Cron;

public class CronTaskService
{
    private readonly LiteDbService _liteDbService;

    public CronTaskService(
        LiteDbService liteDbService)
    {
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

    public void DeleteAllTask()
    {
        _liteDbService.DeleteAll<CronModel>();
    }
}