using AutoMapper;
using BusinessLogicLayer.DTOs;
using DataAccessLayer.Entities;

namespace BusinessLogicLayer.Mappers;

public class OrderAddRequestToOrderMappingProfile : Profile
{
    public OrderAddRequestToOrderMappingProfile()
    {
        CreateMap<OrderAddRequestDto, Order>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(src => src.OrderDate))
            .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.OrderItems))
            .ForMember(dest => dest.OrderId, opt => opt.Ignore())
            .ForMember(dest => dest._id, opt => opt.Ignore())
            .ForMember(dest => dest.TotalBill, opt => opt.Ignore());
    }
}