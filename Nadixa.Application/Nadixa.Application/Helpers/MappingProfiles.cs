using AutoMapper;
using Nadixa.Application.DTOS;
using Nadixa.Application.DTOS.order;
using Nadixa.Application.DTOS.Product;
using Nadixa.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.Helpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            // التحويل من الداتا بيز للـ DTO (عرض البيانات)
            CreateMap<Product, ProductToReturnDto>()
                .ForMember(d => d.Category, o => o.MapFrom(s => s.ProductCategory.Name)) // هات اسم القسم
                .ForMember(d => d.PictureUrl, o => o.MapFrom(s =>
                    s.Images.Any(x => x.IsMain)
                        ? s.Images.First(x => x.IsMain).ImageUrl
                        : s.MainImageUrlPath))
                .ForMember(d => d.GalleryImageUrls, o => o.MapFrom(s =>
                    s.Images.Where(x => !x.IsMain).Select(x => x.ImageUrl)));

            // التحويل من الـ DTO للداتا بيز (إضافة منتج)
            CreateMap<ProductCreateDto, Product>();
            CreateMap<ProductCategory, CategoryToReturnDto>();

            // ============ Order Mappings ============
            CreateMap<Order, OrderDetailsDto>()
                .ForMember(d => d.OrderId, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.Address, opt => opt.MapFrom(s => $"{s.Address}, {s.City}"))
                .ForMember(d => d.Phone, opt => opt.MapFrom(s => s.PhoneNumber))
                .ForMember(d => d.SubTotal, opt => opt.MapFrom(s => s.TotalPrice))
                .ForMember(d => d.GrandTotal, opt => opt.MapFrom(s => s.TotalPrice))
                .ForMember(d => d.AvailableStatuses, opt => opt.Ignore()) // بتتحدد في الـ Service
                .ForMember(d => d.Items, opt => opt.MapFrom(s => s.OrderItems));

            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(d => d.ProductName, opt => opt.MapFrom(s => s.Product.Name))
                .ForMember(d => d.ImageUrl, opt => opt.MapFrom(s => s.Product.MainImageUrlPath));
        }
    }
}
