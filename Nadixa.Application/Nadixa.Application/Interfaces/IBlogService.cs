using Nadixa.Application.DTOS;
using Nadixa.Application.DTOS.Blog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.Interfaces
{
    public interface IBlogService
    {
        Task<BlogListResult> GetAllAsync(int? categoryId, int page, int pageSize);

        Task<BlogDetailDto?> GetDetailAsync(int id);
        Task<int> CreateAsync(BlogCreateDto dto);
        Task<bool> UpdateAsync(BlogEditDto dto);
        Task<bool> DeleteAsync(int id);
        Task<CommentActionResult> AddCommentAsync(int blogId, string content, string userId);
        Task<CommentActionResult> DeleteCommentAsync(int commentId, string userId, bool isAdmin);
        Task<List<CategoryToReturnDto>> GetBlogCategoriesAsync();
    }
}
