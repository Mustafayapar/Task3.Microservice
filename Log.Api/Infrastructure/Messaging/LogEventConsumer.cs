using Log.Api.Application.Abstractions;
using Log.Api.Domain.Entities;
using Log.Api.Domain.Event;
using MassTransit;

namespace Log.Api.Infrastructure.Messaging
{

    public sealed class LogEventConsumer : IConsumer<LogEvent>
    {
        private readonly ILogRepository _repo;
        public LogEventConsumer(ILogRepository repo) => _repo = repo;

        public async Task Consume(ConsumeContext<LogEvent> context)
        {
            var m = context.Message;
            var entry = new LogEntry
            {
                ServiceName = m.ServiceName,
                Level = m.Level,
                Message = m.Message,
                Exception = m.Exception,
                TraceId = m.TraceId,
                TimestampUtc = DateTime.UtcNow
            };

            await _repo.AddAsync(entry, context.CancellationToken);
            await _repo.SaveChangesAsync(context.CancellationToken);
        }
    }
}
