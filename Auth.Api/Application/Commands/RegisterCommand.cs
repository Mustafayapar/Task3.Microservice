using Auth.Api.Application.DTOs;
using Auth.Api.Application.DTOs.ResultDtos;
using MediatR;
namespace Auth.Api.Application.Commands
{
    public sealed record RegisterCommand(string Email, string Password, string FirstName, string LastName)
    : IRequest<RegisterResult>;

}
