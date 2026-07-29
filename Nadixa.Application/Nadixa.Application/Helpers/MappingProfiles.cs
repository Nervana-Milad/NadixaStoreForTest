using AutoMapper;
using Nadixa.Core.DTOS;
using Nadixa.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        }
    }
}
