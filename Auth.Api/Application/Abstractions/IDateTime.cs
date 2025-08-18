namespace Auth.Api.Application.Abstractions
{
    public interface IDateTime
    {
        DateTime UtcNow { get; }
    }
}
