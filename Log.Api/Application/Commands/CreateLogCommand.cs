using MediatR;

namespace Log.Api.Application.Commands
{
    public sealed record CreateLogCommand(
      string ServiceName,
      string Level,
      string Message,
      string? Exception,
      string? TraceId,
      DateTime? TimestampUtc = null
  ) : IRequest<Guid>;
}
