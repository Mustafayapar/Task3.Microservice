namespace Log.Api.Domain.Entities
{
    public class LogEntry
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string ServiceName { get; set; } = null!;
        public string Level { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string? Exception { get; set; }
        public string? TraceId { get; set; }
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    }
}
