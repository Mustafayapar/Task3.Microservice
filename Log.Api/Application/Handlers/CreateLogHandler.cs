using Log.Api.Application.Abstractions;
using Log.Api.Application.Commands;
using Log.Api.Domain.Entities;
using MediatR;

namespace Log.Api.Application.Handlers
{
    public sealed class CreateLogHandler : IRequestHandler<CreateLogCommand, Guid>
    {
        private readonly ILogRepository _repo;
        public CreateLogHandler(ILogRepository repo) => _repo = repo;

        public async Task<Guid> Handle(CreateLogCommand req, CancellationToken ct)
        {
            var entry = new LogEntry
            {
                ServiceName = req.ServiceName,
                Level = req.Level,
                Message = req.Message,
                Exception = req.Exception,
                TraceId = req.TraceId,
                TimestampUtc = req.TimestampUtc ?? DateTime.UtcNow
            };
            await _repo.AddAsync(entry, ct);
            await _repo.SaveChangesAsync(ct);
            return entry.Id;
        }
    }
}
