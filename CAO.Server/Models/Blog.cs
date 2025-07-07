using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAO.Server.Models;

public enum BlogStatus
{
    Draft,
    Published,
    Archived,
}

[Flags]
public enum BlogDisplayType
{
    None = 0,
    Recommended = 1,
    Sticky = 2,
}

[Index(nameof(Slug), IsUnique = true)]
public class Blog
{
    public int Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string Markdown { get; set; } = string.Empty;
    public string Html { get; set; } = string.Empty;
    public BlogStatus Status { get; set; } = BlogStatus.Draft;
    public BlogDisplayType DisplayType { get; set; } = BlogDisplayType.None;
    [NotMapped]
    public bool IsRecommended => DisplayType.HasFlag(BlogDisplayType.Recommended);
    [NotMapped]
    public bool IsSticky => DisplayType.HasFlag(BlogDisplayType.Sticky);
}