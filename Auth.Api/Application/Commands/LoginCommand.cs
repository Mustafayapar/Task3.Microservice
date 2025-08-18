using Auth.Api.Application.DTOs;
using Auth.Api.Application.DTOs.ResultDtos;
using MediatR;

namespace Auth.Api.Application.Commands
{
    public record LoginCommand(string Email, string Password) : IRequest<LoginResult>;

}
