using System.Diagnostics;
using System.Linq.Expressions;
using Hangfire.Storage;
using HangfireDotNetCoreExample.DbService;
using HangfireDotNetCoreExample.Features.Cron;
using HangfireDotNetCoreExample.Features.DevCodes;
using HangfireDotNetCoreExample.Features.Pagination;
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

    private static int MaxId { get; set; }

    public BlogController(
        CronService cronService,
        LiteDbService liteDbService,
        IHubContext<BlogHub> hub)
    {
        _cronService = cronService;
        _liteDbService = liteDbService;
        _hubContext = hub;
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

    public async Task CreateBlog()
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

    public async Task<IActionResult> RunCron(string cron)
    {
        string jobId = Guid.NewGuid().ToString("N");
        _cronService.CreateRecurringJob(
            jobId, () => CreateBlog()
            , cron);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult StopCron(string jobId)
    {
        _cronService.StopRecurringJob(jobId);
        // jobIdList.Remove(jobId);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult StopAllTasks()
    {
        _cronService.RemoveAllRecurringJob();
        return RedirectToAction(nameof(Index));
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
                .GetPagination<BlogDataModel>(pageNo, pageSize, searchingExpression);
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
        model.CronList = cronList;
        model.RunningCrons = cronList.Count; // Need to fixed
        return model;
    }

    private int GetMaxId()
    {
        int result = 0;
        var list = _liteDbService
            .GetList<BlogDataModel>();
        if (list != null && list.Count > 0)
        {
            result = list.Max(x => x.BlogId);
        }

        return result;
    }
}