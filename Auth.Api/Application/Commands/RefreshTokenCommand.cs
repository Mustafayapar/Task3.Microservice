using Auth.Api.Application.DTOs;
using Auth.Api.Application.DTOs.ResultDtos;
using MediatR;

namespace Auth.Api.Application.Commands
{
    public record RefreshTokenCommand(string RefreshToken) : IRequest<LoginResult>;

}
