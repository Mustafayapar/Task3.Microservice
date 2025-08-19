using Log.Api.Domain.Entities;
using MediatR;

namespace Log.Api.Application.Queries
{
    public sealed record GetLogsQuery(
    string? ServiceName = null,
    string? Level = null,
    DateTime? FromUtc = null,
    DateTime? ToUtc = null,
    int Page = 1,
    int PageSize = 50
) : IRequest<PagedLogs>;

    public sealed record PagedLogs(IReadOnlyList<LogEntry> Items, int Total, int Page, int PageSize);
}
