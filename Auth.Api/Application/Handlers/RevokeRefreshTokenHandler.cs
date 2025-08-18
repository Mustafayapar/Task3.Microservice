using Auth.Api.Application.Abstractions;
using Auth.Api.Application.Commands;
using Auth.Api.Infrastructure.Persistence;
using MediatR;

namespace Auth.Api.Application.Handlers
{
    public class RevokeRefreshTokenHandler : IRequestHandler<RevokeRefreshTokenCommand, bool>
    {

        private readonly IUserRepository _userRepository;

        public RevokeRefreshTokenHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> Handle(RevokeRefreshTokenCommand request, CancellationToken cancellationToken)
        {
            await _userRepository.RevokeRefreshTokenAsync(request.UserId, cancellationToken);
            return true;
        }
    }
}
