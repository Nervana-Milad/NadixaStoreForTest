using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nadixa.Application.DTOS;
using Nadixa.Application.DTOS.Blog;
using Nadixa.Application.Interfaces;
using Nadixa.Core.Common;
using Nadixa.Core.Entities;
using Nadixa.Web.Models.ViewModels;

namespace Nadixa.Web.Controllers
{
    public class BlogController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IBlogService _blogService;

        public BlogController(UserManager<AppUser> userManager, IBlogService blogService)
        {
            _userManager = userManager;
            _blogService = blogService;
        }

        public async Task<IActionResult> Index(int? categoryId, int page = 1)
        {
            const int pageSize = 3;

            var result = await _blogService.GetAllAsync(categoryId, page, pageSize);
            ViewBag.Categories = await _blogService.GetBlogCategoriesAsync();
            ViewBag.CurrentPage = result.Page;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.CurrentCategoryId = categoryId;

            return View(result.Blogs);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var blog = await _blogService.GetDetailAsync(id);
            if (blog == null) return NotFound();

            var vm = new BlogDetailViewModel { Blog = blog };
            ViewBag.CurrentUserId = user?.Id;

            return View(vm);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var vm = new BlogCreateViewModel
            {
                Categories = await GetCategorySelectListAsync()
            };

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(BlogCreateViewModel blogViewModel)
        {
            var user = await _userManager.GetUserAsync(User);

            if (!ModelState.IsValid)
            {
                blogViewModel.Categories = await GetCategorySelectListAsync();
                return View(blogViewModel);
            }

            var dto = new BlogCreateDto
            {
                Title = blogViewModel.Title,
                Content = blogViewModel.ShortDescription,
                BlogCategoryId = blogViewModel.BlogCategoryId,
                AuthorId = user.Id,
                Image = blogViewModel.ImageFile != null
                    ? new FileUploadRequest
                    {
                        Content = blogViewModel.ImageFile.OpenReadStream(),
                        FileName = blogViewModel.ImageFile.FileName,
                        Length = blogViewModel.ImageFile.Length
                    }
                    : null
            };

            await _blogService.CreateAsync(dto);

            TempData["Success"] = AppMessages.BlogCreated;
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deleted = await _blogService.DeleteAsync(id);
            if (!deleted) return NotFound();

            TempData["Success"] = AppMessages.BlogDeleted;
            return RedirectToAction("Index", "Blog");
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var blog = await _blogService.GetDetailAsync(id);
            if (blog == null) return NotFound();

            var vm = new BlogEditViewModel
            {
                Id = blog.Id,
                Title = blog.Title,
                Content = blog.Content,
                ExistingImageUrl = blog.ImageUrl,
                BlogCategoryId = blog.BlogCategoryId,
                Categories = await GetCategorySelectListAsync()
            };

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(BlogEditViewModel editViewModel)
        {
            if (!ModelState.IsValid)
            {
                editViewModel.Categories = await GetCategorySelectListAsync();
                return View(editViewModel);
            }

            var dto = new BlogEditDto
            {
                Id = editViewModel.Id,
                Title = editViewModel.Title,
                Content = editViewModel.Content,
                BlogCategoryId = editViewModel.BlogCategoryId,
                NewImage = editViewModel.NewImageFile != null
                    ? new FileUploadRequest
                    {
                        Content = editViewModel.NewImageFile.OpenReadStream(),
                        FileName = editViewModel.NewImageFile.FileName,
                        Length = editViewModel.NewImageFile.Length
                    }
                    : null
            };

            var updated = await _blogService.UpdateAsync(dto);
            if (!updated) return NotFound();

            TempData["Success"] = AppMessages.BlogUpdated;
            return RedirectToAction("Detail", new { id = editViewModel.Id });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddComment(int blogId, string content)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            await _blogService.AddCommentAsync(blogId, content, user.Id);

            TempData["Success"] = AppMessages.CommentBlogCreated;
            return RedirectToAction("Detail", new { id = blogId });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { success = false, message = "Please login first." });

            var result = await _blogService.DeleteCommentAsync(id, user.Id, User.IsInRole("Admin"));

            return Json(new { success = result.Success, message = result.Message });
        }

        // ===== Helper Method =====
        private async Task<List<SelectListItem>> GetCategorySelectListAsync()
        {
            var categories = await _blogService.GetBlogCategoriesAsync();
            return categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();
        }
    }
}