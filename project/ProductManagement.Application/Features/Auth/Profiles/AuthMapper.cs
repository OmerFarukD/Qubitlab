using AutoMapper;
using ProductManagement.Application.Features.Auth.Commands.Login;
using ProductManagement.Application.Features.Auth.Commands.Register;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Application.Features.Auth.Profiles;

public class AuthMapper : Profile
{
    public AuthMapper()
    {
        CreateMap<RegisterCommand, User>();
    }
}
