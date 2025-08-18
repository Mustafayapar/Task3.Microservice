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
    public sealed class RegisterHandler : IRequestHandler<RegisterCommand, RegisterResult>
    {
        private readonly IUserRepository _userRepository;

        public RegisterHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<RegisterResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (existingUser != null)
                return RegisterResult.Failure("Email already in use");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                UserName = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName
            };

            var result = await _userRepository.CreateAsync(user, request.Password, cancellationToken);

            if (!result.Succeeded)
            {
                return RegisterResult.Failure(string.Join(", ", result.Errors));
            }
               

            return RegisterResult.Success();
        }
    }
}
