using Hangfire;
using Hangfire.Storage;
using HangfireDotNetCoreExample.DbService;
using HangfireDotNetCoreExample.Features.Cron;
using HangfireDotNetCoreExample.Models;
using Microsoft.AspNetCore.Mvc;

namespace HangfireDotNetCoreExample.Features.Blog;

public class BlogController : Controller
{
    private readonly CronService _cronService;
    private readonly LiteDbService _liteDbService;

    public BlogController(
        CronService cronService, 
        LiteDbService liteDbService)
    {
        _cronService = cronService;
        _liteDbService = liteDbService;
    }

    public IActionResult Index()
    {
        using var connection = JobStorage.Current.GetConnection();
        List<CronModel> lstCron = connection.GetRecurringJobs()
            .Select(x => Change(x))
            .ToList();
        return View(lstCron);
    }

    private CronModel Change(RecurringJobDto recurringJobDto)
    {
        return new CronModel()
        {
            JobId = recurringJobDto.Id,
            MethodName = $"{recurringJobDto.Job.Method.DeclaringType}.{recurringJobDto.Job.Method.Name}"
        };
    }

    public async Task<IActionResult> RunCron(string cron)
    {
        // Random rand = new Random();
        // string jobId = rand.Next(10, 100).ToString();
        string jobId = Guid.NewGuid().ToString("N");
        _cronService.CreateRecurringJob(
            jobId, () => CreateBlog()
            , cron);
        // jobIdList.Add(jobId);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult StopCron(string jobId)
    {
        _cronService.StopRecurringJob(jobId);
        // jobIdList.Remove(jobId);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult BlogTable()
    {
        var list = _liteDbService.GetList<BlogDataModel>();
        return Json(list);
    }

    public BlogDataModel CreateBlog()
    {
        BlogDataModel model = new BlogDataModel
        {
            BlogAuthor = "Blog Author",
            BlogTitle = "Blog Title",
            BlogContent = "Blog Content",
        };

        _liteDbService.Insert(model);
        return model;
    }
}