using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Auth
{
    public sealed record UserRegisteredEvent(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    DateTime RegisteredAtUtc
);
}
