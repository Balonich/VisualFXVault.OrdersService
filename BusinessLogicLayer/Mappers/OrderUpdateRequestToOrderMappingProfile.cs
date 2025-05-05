using AutoMapper;
using BusinessLogicLayer.DTOs;
using DataAccessLayer.Entities;

namespace BusinessLogicLayer.Mappers;

public class OrderUpdateRequestToOrderMappingProfile : Profile
{
    public OrderUpdateRequestToOrderMappingProfile()
    {
        CreateMap<OrderUpdateRequestDto, Order>()
            .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.OrderId))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(src => src.OrderDate))
            .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.OrderItems))
            .ForMember(dest => dest._id, opt => opt.Ignore())
            .ForMember(dest => dest.TotalBill, opt => opt.Ignore());
    }
}