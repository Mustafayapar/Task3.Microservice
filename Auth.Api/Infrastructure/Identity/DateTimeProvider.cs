using Auth.Api.Application.Abstractions;

namespace Auth.Api.Infrastructure.Identity
{
    public class DateTimeProvider : IDateTime
    {
        public DateTime UtcNow => DateTime.UtcNow;

    }

}
