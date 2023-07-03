using HangFireExample.Features.Blog;
using Microsoft.EntityFrameworkCore;

namespace HangFireExample.EFDbContext;

public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions options) : base(options)
    {
    }
    
    public DbSet<BlogDataModel> Blog { get; set; }
}