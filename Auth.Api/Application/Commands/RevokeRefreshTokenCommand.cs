using MediatR;

namespace Auth.Api.Application.Commands
{
    public record RevokeRefreshTokenCommand(Guid UserId) : IRequest<bool>;

}
