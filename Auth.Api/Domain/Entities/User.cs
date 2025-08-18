using Microsoft.AspNetCore.Identity;

namespace Auth.Api.Domain.Entities
{
    public class User:IdentityUser<Guid>
    {
        public string FirstName { get; set; }=string.Empty;
        public string LastName { get; set; }=string.Empty;
    }
}
