using AutoMapper;
using BusinessLogicLayer.DTOs;
using DataAccessLayer.Entities;

namespace BusinessLogicLayer.Mappers;

public class OrderItemUpdateRequestToOrderItemMappingProfile : Profile
{
    public OrderItemUpdateRequestToOrderItemMappingProfile()
    {
        CreateMap<OrderItemUpdateRequestDto, OrderItem>()
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
            .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice))
            .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
            .ForMember(dest => dest.TotalPrice, opt => opt.Ignore())
            .ForMember(dest => dest._id, opt => opt.Ignore());
    }
}