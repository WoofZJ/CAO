namespace CAO.Shared.Dtos;

public record LoginRequest(
    string Password
);

public record LoginResponse(
    string Token
);