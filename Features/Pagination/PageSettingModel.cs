namespace HangfireDotNetCoreExample.Features.Pagination;

public class PageSettingModel
{
    public int PageNo { get; set; }
    public int PageSize { get; set; }
    public int TotalPageNo { get; set; }
    public int TotalRowCount { get; set; }
    public string PageUrl { get; set; }
    public string SearchParam { get; set; }
    public string GetPageUrl(int pageNo, int pageSize, string SearchParam)
    {
        return string.Format(PageUrl, pageNo, pageSize, SearchParam);
    }
}