using AutoMapper;
using Nadixa.Application.DTOS;
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
                .ForMember(d => d.Category, o => o.MapFrom(s => s.Category.Name)) // هات اسم القسم
                .ForMember(d => d.PictureUrl, o => o.MapFrom(s => s.Images.FirstOrDefault(x => x.IsMain).ImageUrl)); // هات الصورة الرئيسية

            // التحويل من الـ DTO للداتا بيز (إضافة منتج)
            CreateMap<ProductCreateDto, Product>();
        }
    }
}
