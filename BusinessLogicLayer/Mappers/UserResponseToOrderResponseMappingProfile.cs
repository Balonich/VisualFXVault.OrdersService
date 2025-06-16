using AutoMapper;
using BusinessLogicLayer.DTOs;

namespace BusinessLogicLayer.Mappers
{
    public class UserResponseToOrderResponseMappingProfile : Profile
    {
        public UserResponseToOrderResponseMappingProfile()
        {
            CreateMap<UserResponseDto, OrderResponseDto>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));
        }
    }
}