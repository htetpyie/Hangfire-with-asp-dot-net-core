using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HangfireDotNetCoreExample.Features.Blog;

// [Table("Tbl_Blog")]
[Table("Tbl_Hangfire_Blog")]
public class BlogDataModel
{
    [Key] [Column("blog_id")] public int BlogId { get; set; }

    [Column("blog_title")] public string BlogTitle { get; set; }

    [Column("blog_author")] public string BlogAuthor { get; set; }

    [Column("blog_content")] public string BlogContent { get; set; }

    [Column("created_date")] public DateTime CreatedDate { get; set; } = DateTime.Now;

    [Column("created_user")] public int CreatedUser { get; set; }

    [Column("modified_date")] public DateTime? ModifiedDate { get; set; }

    [Column("modified_user")] public int ModifiedUser { get; set; }

    [Column("is_delete")] public bool IsDelete { get; set; }
}