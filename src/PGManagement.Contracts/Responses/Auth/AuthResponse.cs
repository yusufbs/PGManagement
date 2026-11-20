namespace PGManagement.Contracts.Responses.Auth;

public record AuthResponse(
    string Token,
    string RefreshToken,
    DateTime ExpiresAt,
    string Email,
    string FullName
);
