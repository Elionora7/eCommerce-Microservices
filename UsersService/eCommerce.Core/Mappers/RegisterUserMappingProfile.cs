using AutoMapper;
using eCommerce.Core.DTO;
using eCommerce.Core.Entities;

namespace eCommerce.Core.Mappers;

public class RegisterUserMappingProfile : Profile
{
    public RegisterUserMappingProfile()
    {
        CreateMap<RegisterRequest, ApplicationUser>()
          .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
          .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
          .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender.ToString()))
          .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password))
          ;
    }
}