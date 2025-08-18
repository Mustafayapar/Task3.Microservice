using Auth.Api.Application.Abstractions;
using Auth.Api.Application.Commands;
using Auth.Api.Application.DTOs;
using Auth.Api.Application.DTOs.ResultDtos;
using Auth.Api.Domain.Entities;
using Auth.Api.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Auth.Api.Application.Handlers
{
    public sealed class LoginHandler : IRequestHandler<LoginCommand, LoginResult>
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public LoginHandler(IUserRepository userRepository, IJwtTokenGenerator jwtTokenGenerator)
        {
            _userRepository = userRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user == null)
                return LoginResult.Failure("User not found");

            // Şifre kontrolü
            var isPasswordValid = await _userRepository.CheckPasswordAsync(user, request.Password, cancellationToken);
            if (!isPasswordValid)
                return LoginResult.Failure("Invalid credentials");

            // Access + Refresh Token üretimi
            var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);
            var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();
            var refreshTokenExpire = DateTime.UtcNow.AddDays(7); 

            await _userRepository.SaveRefreshTokenAsync(user.Id, refreshToken, refreshTokenExpire, cancellationToken);

            return LoginResult.SuccessResult(accessToken, refreshToken);
        }
    }
}