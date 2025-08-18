using Auth.Api.Domain.Entities;

namespace Auth.Api.Application.Abstractions
{
    public interface IRefreshTokenRepository
    {
        Task AddAsync(RefreshToken token);
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task DeleteAsync(RefreshToken token);
    }
}
