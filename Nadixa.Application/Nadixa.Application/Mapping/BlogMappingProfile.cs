using AutoMapper;
using Nadixa.Application.DTOS.Blog;
using Nadixa.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.Mapping
{
    public class BlogMappingProfile : Profile
    {
        public BlogMappingProfile()
        {
            // Comments: نسخ مباشر بالكامل
            CreateMap<BlogComment, BlogCommentDto>()
                .ForMember(d => d.AuthorId, o => o.MapFrom(s => s.AppUserId))
                .ForMember(d => d.AuthorName, o => o.MapFrom(s => s.AppUser.FirstName + " " + s.AppUser.LastName));

            // BlogDetailDto: نسخ مباشر، الـ Comments بتتحط يدوي في الـ Service لأنها List منفصلة
            CreateMap<Blog, BlogDetailDto>()
                .ForMember(d => d.Category, o => o.MapFrom(s => s.BlogCategory.Name))
                .ForMember(d => d.Comments, o => o.Ignore());

            // BlogListItemDto فيها حساب (substring للـ ShortDescription + Count)، فهنعملها Manual في الـ Service
        }
    }
}
