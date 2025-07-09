namespace CAO.Shared.Dtos;

public record ArchiveResponse(
    string Slug,
    string Title,
    DateTime CreatedAt,
    DateTime UpdatedAt
);