using AutoMapper;
using Nadixa.Application.DTOS.ProductCategory;
using Nadixa.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.Mapping
{
    public class CategoryMappingProfile : Profile
    {
        public CategoryMappingProfile()
        {
            // نسخ مباشر بالكامل، مفيش أي قرار منطقي
            CreateMap<ProductCategory, CategoryDto>();
        }
    }
}
