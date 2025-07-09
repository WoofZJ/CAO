namespace CAO.Shared.Dtos;

public record TagItemResponse(
    string Slug,
    string Title,
    DateTime UpdatedAt
);