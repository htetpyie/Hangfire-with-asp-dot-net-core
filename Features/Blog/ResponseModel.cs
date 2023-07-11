using HangfireDotNetCoreExample.Features.Cron;

namespace HangfireDotNetCoreExample.Features.Blog;

public class ResponseModel
{
    public BlogListResponseModel BlogListResponse { get; set; }
    public CronResponseModel CronResponse { get; set; }
}