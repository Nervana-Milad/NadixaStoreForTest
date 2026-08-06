using AutoMapper;
using Nadixa.Application.DTOS.ProductSubCategory;
using Nadixa.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.Mapping
{
    public class SubCategoryMappingProfile : Profile
    {
        public SubCategoryMappingProfile()
        {
            CreateMap<ProductSubCategory, SubCategoryDto>()
                .ForMember(d => d.ProductCategoryName, o => o.MapFrom(s => s.ProductCategory.Name));
        }
    }
}
