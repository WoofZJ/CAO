namespace CAO.Shared.Dtos;

public record BlogMetadataResponse
(
    string Slug,
    string Title,
    string Description,
    string ImageUrl,
    List<string> Tags,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    bool IsRecommended,
    bool IsSticky
);