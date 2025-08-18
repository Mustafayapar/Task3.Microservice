using Auth.Api.Domain.Entities;

namespace Auth.Api.Application.Abstractions
{
    public interface IJwtTokenGenerator
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
    }
}
