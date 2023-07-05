using LiteDB;

namespace HangfireDotNetCoreExample.Features.Blog;

public class BlogDataModel
{
    [BsonId]
    public int BlogId { get; set; }

    public string BlogTitle { get; set; }

    public string BlogAuthor { get; set; }

    public string BlogContent { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    public int CreatedUser { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public int ModifiedUser { get; set; }

    public bool IsDelete { get; set; }
}