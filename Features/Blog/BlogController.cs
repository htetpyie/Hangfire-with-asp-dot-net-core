using System.Diagnostics;
using Hangfire;
using Hangfire.Storage;
using HangfireDotNetCoreExample.DbService;
using HangfireDotNetCoreExample.Features.Cron;
using HangfireDotNetCoreExample.Features.SignalRHubs;
using HangfireDotNetCoreExample.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace HangfireDotNetCoreExample.Features.Blog;

public class BlogController : Controller
{
    private readonly CronService _cronService;
    private readonly LiteDbService _liteDbService;
    private readonly IHubContext<BlogHub> _hubContext;

    public BlogController(
        CronService cronService,
        LiteDbService liteDbService, IHubContext<BlogHub> hub)
    {
        _cronService = cronService;
        _liteDbService = liteDbService;
        _hubContext = hub;
    }

    public IActionResult Index()
    {
        using var connection = JobStorage.Current.GetConnection();
        List<CronModel> lstCron = connection.GetRecurringJobs()
            .Select(x => Change(x))
            .ToList();
        return View(lstCron);
    }

    public IActionResult BlogTable()
    {
        var list = GetList();
        return Json(list);
    }

    public async Task<BlogDataModel> CreateBlog()
    {
        BlogDataModel model = new BlogDataModel
        {
            BlogAuthor = "Blog Author",
            BlogTitle = "Blog Title",
            BlogContent = "Blog Content",
        };
        try
        {
            _liteDbService.Insert(model);
            await SendList();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return model;
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

    private async Task SendList()
    {
        var list = GetList();
        await _hubContext.Clients.All.SendAsync("RecieveList", list);
    }

    private List<BlogDataModel> GetList()
    {
        List<BlogDataModel> list = new List<BlogDataModel>();
        try
        {
            list = _liteDbService.GetList<BlogDataModel>();
        }
        catch (Exception e)
        {
            Debug.WriteLine("Exception in List => " + e.Message);
        }

        return list;
    }

    private CronModel Change(RecurringJobDto recurringJobDto)
    {
        return new CronModel()
        {
            JobId = recurringJobDto.Id,
            MethodName = $"{recurringJobDto.Job.Method.DeclaringType}.{recurringJobDto.Job.Method.Name}"
        };
    }
}