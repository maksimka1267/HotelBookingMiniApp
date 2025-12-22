namespace HotelBooking.Data.Application.Dto;

public sealed record RegisterRequest(string Email, string Password);
public sealed record LoginRequest(string Email, string Password);
public sealed record TokenResponse(string Token);

public sealed record MeDto(
    bool IsAuth,
    Guid? UserId,
    string? Email,
    IReadOnlyList<string> Roles
);
