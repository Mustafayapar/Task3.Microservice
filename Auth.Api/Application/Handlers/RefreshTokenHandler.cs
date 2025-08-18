using Auth.Api.Application.Abstractions;
using Auth.Api.Application.Commands;
using Auth.Api.Application.DTOs;
using Auth.Api.Application.DTOs.ResultDtos;
using Auth.Api.Domain.Entities;
using Auth.Api.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Auth.Api.Application.Handlers
{
    public sealed class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, LoginResult>
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public RefreshTokenHandler(IUserRepository userRepository, IJwtTokenGenerator jwtTokenGenerator)
        {
            _userRepository = userRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<LoginResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            // Refresh token geçerli mi?
            var user = await _userRepository.GetUserByRefreshTokenAsync(request.RefreshToken, cancellationToken);
            if (user == null)
                return LoginResult.Failure("Invalid refresh token");

            // Yeni Access & Refresh token üret
            var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);
            var newRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();

            await _userRepository.SaveRefreshTokenAsync(
                user.Id,
                newRefreshToken,
                DateTime.UtcNow.AddDays(7),
                cancellationToken);

            return LoginResult.SuccessResult(accessToken, newRefreshToken);
        }
    }
}
