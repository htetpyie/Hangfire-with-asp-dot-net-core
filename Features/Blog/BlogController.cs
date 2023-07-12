using System.Diagnostics;
using System.Linq.Expressions;
using HangfireDotNetCoreExample.DbService;
using HangfireDotNetCoreExample.Features.Cron;
using HangfireDotNetCoreExample.Features.DevCodes;
using HangfireDotNetCoreExample.Features.Pagination;
using HangfireDotNetCoreExample.Features.SignalRHubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace HangfireDotNetCoreExample.Features.Blog;

public class BlogController : Controller
{
    private readonly CronService _cronService;
    private readonly LiteDbService _liteDbService;
    private readonly IHubContext<BlogHub> _hubContext;
    private readonly CronTaskService _cronTaskService;

    private static int MaxId { get; set; }

    public BlogController(
        CronService cronService,
        LiteDbService liteDbService,
        IHubContext<BlogHub> hub, CronTaskService cronTaskService)
    {
        _cronService = cronService;
        _liteDbService = liteDbService;
        _hubContext = hub;
        _cronTaskService = cronTaskService;
    }

    public IActionResult Index()
    {
        var cronResponse = GetCronResponseModel();
        var blogListResponse = GetBlogListResponseModel();

        var response = new ResponseModel
        {
            CronResponse = cronResponse,
            BlogListResponse = blogListResponse
        };

        return View(response);
    }

    public async Task<IActionResult> GenerateList(int size = 50)
    {
        for (int i = 0; i < size; i++)
        {
            await CreateBlog();
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> ClearAllList()
    {
        MaxId = GetMaxId();
        _liteDbService.DeleteAll<BlogDataModel>();
        return RedirectToAction(nameof(Index));
    }

    public IActionResult BlogTable(
        int pageNo = 1,
        int pageSize = 10,
        string searchParam = "")
    {
        var list = GetList(pageNo, pageSize, searchParam);
        var totalRowCount = GetTotalRowCount(searchParam);

        var pageSetting = new PageSettingModel
        {
            PageNo = pageNo,
            PageSize = pageSize,
            SearchParam = searchParam,
            TotalRowCount = totalRowCount
        };
        BlogListResponseModel response = new BlogListResponseModel
        {
            BlogList = list,
            PageSettingModel = pageSetting
        };

        return View(response);
    }

    public IActionResult RunCron(string cron)
    {
        string jobId = Guid.NewGuid().ToString("N");
        _cronService.CreateRecurringJob(
            jobId, () => CreateOneBlog()
            , cron);

        return RedirectToAction(nameof(Index));
    }

    public IActionResult StopCron(string jobId)
    {
        var cron = _cronService.StopRecurringJob(jobId);
        _cronTaskService.SaveTask(cron);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult StopAllTasks()
    {
        var list = _cronService.RemoveAllRecurringJob();
        list.ForEach(x => _cronTaskService.SaveTask(x));
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> CreateOneBlog()
    {
        await CreateBlog();
        return RedirectToAction(nameof(Index));
    }

    public IActionResult GetCronCount()
    {
        var model = new CronCountModel();
        model.RunningCrons = _cronService
            .GetAllCronList()
            .Count(x => x.IsRunning)
            .ToString("N0");
        model.StoppedCrons = _cronTaskService
            .GetAllStoppedTask()
            .Count
            .ToString("N0");
        
        return Json(model);
    }

    private async Task CreateBlog()
    {
        try
        {
            int lastId = GetMaxId() == 0
                    ? (MaxId + 1)
                    : (GetMaxId() + 1)
                ;

            BlogDataModel model = new BlogDataModel
            {
                BlogAuthor = "Blog Author " + lastId,
                BlogTitle = "Blog Title " + lastId,
                BlogContent = "Blog Content " + lastId,
            };
            _liteDbService.Insert(model);
            await SendList();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private async Task SendList()
    {
        var list = GetList();
        await _hubContext
            .Clients
            .All
            .SendAsync("ReceiveList", list);
    }

    private List<BlogDataModel> GetList(
        int pageNo = 1,
        int pageSize = 10,
        string searchParam = "")
    {
        List<BlogDataModel> list = new List<BlogDataModel>();
        try
        {
            var searchingExpression = GetExpression(searchParam);
            list = _liteDbService
                .GetPagination1<BlogDataModel>(
                    pageNo,
                    pageSize,
                    searchingExpression,
                    model => model.BlogId);
        }
        catch (Exception e)
        {
            Debug.WriteLine("Exception in List => " + e.Message);
        }

        return list;
    }

    private int GetTotalRowCount(string searchParam)
    {
        int pageCount = 0;
        try
        {
            var searchingExpression = GetExpression(searchParam);
            pageCount = _liteDbService.GetTotalRowCount(searchingExpression);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return pageCount;
    }

    private Expression<Func<BlogDataModel, bool>> GetExpression(string searchParam)
    {
        Expression<Func<BlogDataModel, bool>> searching = (x => true);
        if (!searchParam.Trim().IsNullOrEmpty())
        {
            searchParam = searchParam.Trim().ToLower();
            searching = (x =>
                x.BlogTitle.ToLower().Contains(searchParam) ||
                x.BlogAuthor.ToLower().Contains(searchParam) ||
                x.BlogContent.ToLower().Contains(searchParam));
        }

        return searching;
    }

    private BlogListResponseModel GetBlogListResponseModel()
    {
        var list = GetList();
        var pageSetting = new PageSettingModel
        {
            PageNo = 1,
            PageSize = 10,
            SearchParam = "",
            TotalRowCount = GetTotalRowCount("")
        };
        BlogListResponseModel response = new BlogListResponseModel
        {
            BlogList = list,
            PageSettingModel = pageSetting
        };
        return response;
    }

    private CronResponseModel GetCronResponseModel()
    {
        CronResponseModel model = new();
        var cronList = _cronService.GetAllCronList();
        var stoppedList = _cronTaskService.GetAllStoppedTask();
        model.RunningCronList = cronList;
        model.StoppedCronList = stoppedList;
        return model;
    }

    private int GetMaxId()
    {
        int result = 0;
        var list = _liteDbService
            .GetList<BlogDataModel>();
        if (list.Count > 0)
        {
            result = list.Max(x => x.BlogId);
        }

        return result;
    }
}