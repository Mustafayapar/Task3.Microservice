using Log.Api.Application.Abstractions;
using Log.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Log.Api.Infrastructure.Persistence
{
    public sealed class LogRepository : ILogRepository
    {
        private readonly LogDbContext _db;
        public LogRepository(LogDbContext db) => _db = db;

        public Task AddAsync(LogEntry entry, CancellationToken ct = default)
            => _db.Logs.AddAsync(entry, ct).AsTask();

        public Task SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);

        public async Task<(IReadOnlyList<LogEntry>, int)> GetAsync(
            string? serviceName, string? level, DateTime? fromUtc, DateTime? toUtc,
            int page, int pageSize, CancellationToken ct = default)
        {
            var q = _db.Logs.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(serviceName))
                q = q.Where(x => x.ServiceName == serviceName);

            if (!string.IsNullOrWhiteSpace(level))
                q = q.Where(x => x.Level == level);

            if (fromUtc.HasValue)
                q = q.Where(x => x.TimestampUtc >= fromUtc.Value);

            if (toUtc.HasValue)
                q = q.Where(x => x.TimestampUtc <= toUtc.Value);

            q = q.OrderByDescending(x => x.TimestampUtc);

            var total = await q.CountAsync(ct);
            var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
            return (items, total);
        }
    }
}
