using HangFireExample.EFDbContext;
using HangFireExample.Features.Blog;
using HangFireExample.HangFire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HangFireExample.Features.Blog;

public class BlogController : Controller
{
    private readonly AppDbContext _context;
    private readonly HangFireService _hangFireService;
    public static List<string> jobIdList = new List<string>();

    public BlogController(AppDbContext context,
        HangFireService hangFireService)
    {
        _context = context;
        _hangFireService = hangFireService;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult RunCron(string cron)
    {
        Random rand = new Random();
        string jobId = rand.Next(10, 100).ToString();
        _hangFireService.CreateRecurringJob(
            jobId,
            () => CreateBlog()
            , cron);
        jobIdList.Add(jobId);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult StopCron(string jobId)
    {
        _hangFireService.StopRecurringJob(jobId);
        jobIdList.Remove(jobId);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult BlogTable()
    {
        var list = _context.Blog.AsNoTracking().ToList();
        return Json(list ?? new List<BlogDataModel>());
    }

    public void CreateBlog()
    {
        BlogDataModel model = new BlogDataModel
        {
            BlogAuthor = "Blog Author",
            BlogTitle = "Blog Title",
            BlogContent = "Blog Content",
        };

        _context.Blog.Add(model);
        _context.SaveChanges();
    }
}