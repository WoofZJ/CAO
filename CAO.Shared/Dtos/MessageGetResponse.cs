namespace CAO.Shared.Dtos;

public record MessageGetResponse(
    string Nickname,
    string Content,
    int AvatarId,
    DateTime CreatedAt,
    int Status
);