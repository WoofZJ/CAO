namespace CAO.Shared.Dtos;

public record BlogListItemDto(
    int Id,
    string Title,
    string Slug,
    string Description,
    string ImageUrl,
    List<string> Tags,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int Status,
    bool IsRecommended,
    bool IsSticky
);

public record BlogMarkdownDto(
    string Slug,
    string Markdown
);