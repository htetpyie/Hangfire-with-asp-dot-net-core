namespace HangfireDotNetCoreExample.Features.Pagination;

public class PageSettingModel
{
    public int PageNo { get; set; }
    public int PageSize { get; set; }
    public int TotalRowCount { get; set; }

    public int TotalPageNo
    {
        get => (TotalRowCount % PageSize) == 0
            ? (TotalRowCount / PageSize)
            : (TotalRowCount / PageSize) + 1;
    }

    public string PageUrl { get; set; }
    public string SearchParam { get; set; }

    public string GetPageUrl(int pageNo, int pageSize, string SearchParam)
    {
        return string.Format(PageUrl, pageNo, pageSize, SearchParam);
    }
}