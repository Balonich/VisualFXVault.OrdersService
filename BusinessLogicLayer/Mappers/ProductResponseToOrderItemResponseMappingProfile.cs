using AutoMapper;
using BusinessLogicLayer.DTOs;

namespace BusinessLogicLayer.Mappers
{
    public class ProductResponseToOrderItemResponseMappingProfile : Profile
    {
        public ProductResponseToOrderItemResponseMappingProfile()
        {
            CreateMap<ProductResponseDto, OrderItemResponseDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductName))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category));
        }
    }
}