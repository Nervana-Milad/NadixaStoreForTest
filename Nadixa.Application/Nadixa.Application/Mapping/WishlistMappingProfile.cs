using AutoMapper;
using Nadixa.Application.DTOS;
using Nadixa.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.Mapping
{
    public class WishlistMappingProfile : Profile
    {
        public WishlistMappingProfile()
        {
            CreateMap<WishlistItem, WishlistItemDto>()
                .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product.Name))
                .ForMember(d => d.Price, o => o.MapFrom(s => s.Product.Price))
                .ForMember(d => d.MainImageUrl, o => o.MapFrom(s => s.Product.MainImageUrlPath))
                .ForMember(d => d.StockQuantity, o => o.MapFrom(s => s.Product.StockQuantity))
                .ForMember(d => d.CartQuantity, o => o.Ignore());
        }
    }
}
