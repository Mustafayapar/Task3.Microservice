using Auth.Api.Application.Abstractions;
using Auth.Api.Domain.Entities;
using Auth.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Auth.Api.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<User> _userManager;
        private readonly AuthDbContext _dbContext;

        public UserRepository(UserManager<User> userManager, AuthDbContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        {
            return await _userManager.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
        }

        public async Task<bool> CheckPasswordAsync(User user, string password, CancellationToken ct = default)
        {
            // UserManager metodları CancellationToken almıyor → direkt çağırıyoruz
            return await _userManager.CheckPasswordAsync(user, password);
        }

        public async Task SaveRefreshTokenAsync(Guid userId, string refreshToken, DateTime expiresAt, CancellationToken ct = default)
        {
            var token = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Token = refreshToken,
                ExpiresAt = expiresAt,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.RefreshTokens.Add(token);
            await _dbContext.SaveChangesAsync(ct);
        }

        public async Task<string?> GetRefreshTokenAsync(Guid userId, CancellationToken ct = default)
        {
            var token = await _dbContext.RefreshTokens
                .Where(r => r.UserId == userId && r.ExpiresAt > DateTime.UtcNow && r.RevokedAt == null)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync(ct);

            return token?.Token;
        }

        public async Task<User?> GetUserByRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
        {
            var token = await _dbContext.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == refreshToken && r.ExpiresAt > DateTime.UtcNow && r.RevokedAt == null, ct);

            if (token == null) return null;

            return await _userManager.FindByIdAsync(token.UserId.ToString());
        }

        public async Task RevokeRefreshTokenAsync(Guid userId, CancellationToken ct = default)
        {
            var tokens = await _dbContext.RefreshTokens
                .Where(r => r.UserId == userId && r.RevokedAt == null)
                .ToListAsync(ct);

            foreach (var token in tokens)
            {
                token.RevokedAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync(ct);
        }
        public async Task<IdentityResult> CreateAsync(User user, string password, CancellationToken ct = default)
        {
            return await _userManager.CreateAsync(user, password);
        }
    }
}
