using Auth.Api.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Auth.Api.Application.Abstractions
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> CheckPasswordAsync(User user, string password, CancellationToken cancellationToken = default);
        Task SaveRefreshTokenAsync(Guid userId, string refreshToken, DateTime expiresAt, CancellationToken cancellationToken = default);
        Task<string?> GetRefreshTokenAsync(Guid userId, CancellationToken cancellationToken = default);
        Task RevokeRefreshTokenAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<User?> GetUserByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
        Task<IdentityResult> CreateAsync(User user, string password, CancellationToken ct = default);

    }
}