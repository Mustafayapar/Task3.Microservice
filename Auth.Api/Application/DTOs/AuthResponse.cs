namespace Auth.Api.Application.DTOs
{
    public record AuthResponse(
    string AccessToken,
    long ExpiresIn,
    string RefreshToken
);
}
