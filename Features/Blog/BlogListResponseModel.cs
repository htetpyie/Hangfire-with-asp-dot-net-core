using HangfireDotNetCoreExample.Features.Pagination;

namespace HangfireDotNetCoreExample.Features.Blog;

public class BlogListResponseModel
{
    public List<BlogDataModel> BlogList { get; set; }
    public PageSettingModel PageSettingModel;
}