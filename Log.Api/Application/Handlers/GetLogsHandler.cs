using Log.Api.Application.Abstractions;
using Log.Api.Application.Queries;
using MediatR;

namespace Log.Api.Application.Handlers
{
    public sealed class GetLogsHandler : IRequestHandler<GetLogsQuery, PagedLogs>
    {
        private readonly ILogRepository _repo;
        public GetLogsHandler(ILogRepository repo) => _repo = repo;

        public async Task<PagedLogs> Handle(GetLogsQuery q, CancellationToken ct)
        {
            var (items, total) = await _repo.GetAsync(
                q.ServiceName, q.Level, q.FromUtc, q.ToUtc,
                Math.Max(1, q.Page), Math.Clamp(q.PageSize, 1, 500), ct);

            return new PagedLogs(items, total, Math.Max(1, q.Page), Math.Clamp(q.PageSize, 1, 500));
        }
    }
}
