namespace CAO.Shared.Dtos;

public record MessagePostRequest(
    string Nickname,
    string Email,
    string Content,
    int AvatarId,
    Guid SessionId
);