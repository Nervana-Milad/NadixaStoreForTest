using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nadixa.Core.Entities;
using Nadixa.Application.DTOS.Cart;

namespace Nadixa.Application.Mapping
{
    public class CartMappingProfile : Profile
    {
        public CartMappingProfile()
        {
            CreateMap<CartItem, CartItemDto>()
                .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product.Name))
                .ForMember(d => d.Price, o => o.MapFrom(s => s.Product.Price))
                .ForMember(d => d.StockQuantity, o => o.MapFrom(s => s.Product.StockQuantity))
                .ForMember(d => d.MainImageUrl, o => o.MapFrom(s => s.Product.MainImageUrlPath))
                .ForMember(d => d.PromoBadgeText, o => o.Ignore())
                .ForMember(d => d.PromoBadgeColorHex, o => o.Ignore())
                .ForMember(d => d.DiscountedUnitPrice, o => o.Ignore());
    }
}
}
