using Log.Api.Domain.Entities;

namespace Log.Api.Application.Abstractions
{
    public interface ILogRepository
    {
        Task AddAsync(LogEntry entry, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
        Task<(IReadOnlyList<LogEntry> Items, int Total)> GetAsync(
            string? serviceName, string? level, DateTime? fromUtc, DateTime? toUtc,
            int page, int pageSize, CancellationToken ct = default);
    }
}
