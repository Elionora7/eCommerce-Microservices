

using AutoMapper;
using eCommerce.Core.DTO;
using eCommerce.Core.Entities;

namespace eCommerce.Core.Mappers;

public class ApplicationUserToUserDTOMappingProfile : Profile
{
    public ApplicationUserToUserDTOMappingProfile()
    {
        CreateMap<ApplicationUser, UserDTO>()
            .ForMember(des => des.UserID, opt => opt.MapFrom(src => src.UserID))
            .ForMember(des => des.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(des => des.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(des => des.Gender, opt => opt.MapFrom(src => src.Gender))
            .ForMember(dest => dest.RefreshToken, opt => opt.MapFrom(src => src.RefreshToken))
            .ForMember(dest => dest.RefreshTokenExpiryTime, opt => opt.MapFrom(src => src.RefreshTokenExpiryTime));



    }
}
