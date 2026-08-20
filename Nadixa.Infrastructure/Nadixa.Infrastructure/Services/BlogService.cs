using AutoMapper;
using Nadixa.Application.DTOS;
using Nadixa.Application.DTOS.Blog;
using Nadixa.Application.Interfaces;
using Nadixa.Core.Entities;
using Nadixa.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Infrastructure.Services
{
    public class BlogService : IBlogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileUploadService _fileUploadService;
        private readonly IMapper _mapper;
        public BlogService(IUnitOfWork unitOfWork, IFileUploadService fileUploadService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _fileUploadService = fileUploadService;
            _mapper = mapper;
        }

        public async Task<BlogListResult> GetAllAsync(int? categoryId, int page, int pageSize)
        {
            var blogs = categoryId.HasValue
                ? await _unitOfWork.Repository<Blog>()
                    .FindAsync(b => b.BlogCategoryId == categoryId.Value,
                        b => b.BlogCategory, b => b.BlogComments, b => b.AppUser)
                : await _unitOfWork.Repository<Blog>()
                    .GetAllAsync(b => b.BlogCategory, b => b.BlogComments, b => b.AppUser);

            var ordered = blogs.OrderByDescending(b => b.CreateAt).ToList();

            int totalCount = ordered.Count;

            var pagedBlogs = ordered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var blogDtos = pagedBlogs.Select(b => new BlogListItemDto
            {
                Id = b.Id,
                Title = b.Title,
                ImageUrl = b.ImageUrl,
                Author = $"{b.AppUser.FirstName} {b.AppUser.LastName}",
                Category = b.BlogCategory.Name,
                ShortDescription = b.Content.Length > 100 ? b.Content.Substring(0, 100) : b.Content,
                Date = b.CreateAt,
                CommentsCount = b.BlogComments.Count
            }).ToList();

            return new BlogListResult
            {
                Blogs = blogDtos,
                TotalCount = totalCount,
                Page = page,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
        }
        //public async Task<List<BlogListItemDto>> GetAllAsync(int? categoryId)
        //{
        //    var blogs = categoryId.HasValue
        //        ? await _unitOfWork.Repository<Blog>()
        //            .FindAsync(b => b.BlogCategoryId == categoryId.Value,
        //                b => b.BlogCategory, b => b.BlogComments, b => b.AppUser)
        //        : await _unitOfWork.Repository<Blog>()
        //            .GetAllAsync(b => b.BlogCategory, b => b.BlogComments, b => b.AppUser);

        //    var ordered = blogs.OrderByDescending(b => b.CreateAt).ToList();

        //    return ordered.Select(b => new BlogListItemDto
        //    {
        //        Id = b.Id,
        //        Title = b.Title,
        //        ImageUrl = b.ImageUrl,
        //        Author = $"{b.AppUser.FirstName} {b.AppUser.LastName}",
        //        Category = b.BlogCategory.Name,
        //        ShortDescription = b.Content.Length > 100 ? b.Content.Substring(0, 100) : b.Content,
        //        Date = b.CreateAt,
        //        CommentsCount = b.BlogComments.Count
        //    }).ToList();
        //}

        public async Task<BlogDetailDto> GetDetailAsync(int id)
        {
            var blog = await _unitOfWork.Repository<Blog>()
                .GetByIdAsync(id, b => b.BlogCategory, b => b.BlogComments, b => b.AppUser);

            if (blog == null) return null;

            var comments = await _unitOfWork.Repository<BlogComment>()
                .FindAsync(c => c.BlogId == id, c => c.AppUser);

            var dto = _mapper.Map<BlogDetailDto>(blog);
            dto.Comments = _mapper.Map<List<BlogCommentDto>>(comments.OrderBy(c => c.CreatedAt));

            return dto;
        }
    
        public async Task<int> CreateAsync(BlogCreateDto dto)
        {
            string? imagePath = null;

            if (dto.Image != null)
            {
                imagePath = await _fileUploadService.UploadImageAsync(
                    dto.Image.Content, dto.Image.FileName, dto.Image.Length, "blogs");
            }

            var blog = new Blog
            {
                Title = dto.Title,
                Content = dto.Content,
                ImageUrl = imagePath,
                CreateAt = DateTime.Now,
                AppUserId = dto.AuthorId,
                BlogCategoryId = dto.BlogCategoryId
            };

            await _unitOfWork.Repository<Blog>().AddAsync(blog);
            await _unitOfWork.CompleteAsync();

            return blog.Id;
        }


        public async Task<bool> UpdateAsync(BlogEditDto dto)
        {
            var blog = await _unitOfWork.Repository<Blog>().GetByIdAsync(dto.Id);
            if (blog == null) return false;

            if (dto.NewImage != null)
            {
                if (!string.IsNullOrEmpty(blog.ImageUrl))
                    _fileUploadService.DeleteFile(blog.ImageUrl);

                blog.ImageUrl = await _fileUploadService.UploadImageAsync(
                    dto.NewImage.Content, dto.NewImage.FileName, dto.NewImage.Length, "blogs");
            }

            blog.Title = dto.Title;
            blog.Content = dto.Content;
            blog.BlogCategoryId = dto.BlogCategoryId;

            _unitOfWork.Repository<Blog>().Update(blog);
            await _unitOfWork.CompleteAsync();

            return true;
        }


        public async Task<bool> DeleteAsync(int id)
        {
            var blog = await _unitOfWork.Repository<Blog>().GetByIdAsync(id);
            if (blog == null) return false;

            if (!string.IsNullOrEmpty(blog.ImageUrl))
                _fileUploadService.DeleteFile(blog.ImageUrl);

            _unitOfWork.Repository<Blog>().Delete(blog);   
            await _unitOfWork.CompleteAsync();

            return true;
        }


        public async Task<CommentActionResult> AddCommentAsync(int blogId, string content, string userId)
        {
            if (string.IsNullOrWhiteSpace(content))
                return new CommentActionResult { Success = false, Message = "Comment cannot be empty." };

            var comment = new BlogComment
            {
                Content = content,
                BlogId = blogId,
                AppUserId = userId,
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.Repository<BlogComment>().AddAsync(comment);
            await _unitOfWork.CompleteAsync();

            return new CommentActionResult { Success = true };

        }


        public async Task<CommentActionResult> DeleteCommentAsync(int commentId, string userId, bool isAdmin)
        {
            var comment = await _unitOfWork.Repository<BlogComment>().GetByIdAsync(commentId);
            if (comment == null)
                return new CommentActionResult { Success = false, Message = "Comment not found." };

            if (comment.AppUserId != userId && !isAdmin)
                return new CommentActionResult { Success = false, Message = "Not allowed." };

            _unitOfWork.Repository<BlogComment>().HardDelete(comment);
            await _unitOfWork.CompleteAsync();

            return new CommentActionResult { Success = true };
        }


        public async Task<List<CategoryToReturnDto>> GetBlogCategoriesAsync()
        {
            var categories = await _unitOfWork.Repository<BlogCategory>().GetAllAsync();

            return categories.Select(c => new CategoryToReturnDto
            {
                Id = c.Id,
                Name = c.Name
            }).ToList();
        }
    }
}
