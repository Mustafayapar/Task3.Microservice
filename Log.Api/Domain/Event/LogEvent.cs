namespace Log.Api.Domain.Event
{
    public sealed record LogEvent(string ServiceName, string Level, string Message, string? Exception, string? TraceId);

}
