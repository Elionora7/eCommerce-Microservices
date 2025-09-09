using AutoMapper;
using eCommerce.Core.DTO;
using eCommerce.Core.Entities;

namespace eCommerce.Core.Mappers;

public class ApplicationUserMappingProfile : Profile
{
    public ApplicationUserMappingProfile()
    {
        CreateMap<ApplicationUser, AuthenticationResponse>()
          .ForMember(dest => dest.UserID, opt => opt.MapFrom(src => src.UserID))
          .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
          .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
          .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
          .ForMember(dest => dest.RefreshToken, opt => opt.MapFrom(src => src.RefreshToken))
          .ForMember(dest => dest.RefreshTokenExpiryTime, opt => opt.MapFrom(src => src.RefreshTokenExpiryTime))
          .ForMember(dest => dest.Token, opt => opt.Ignore())
          .ForMember(dest => dest.Success, opt => opt.Ignore())
          
          ;
    }
}