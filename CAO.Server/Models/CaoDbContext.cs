using Microsoft.EntityFrameworkCore;

namespace CAO.Server.Models;

public class CaoDbContext(DbContextOptions<CaoDbContext> options)
    : DbContext(options)
{
    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Visit> Visits { get; set; }
    public DbSet<VisitorInfo> VisitorInfos { get; set; }
}