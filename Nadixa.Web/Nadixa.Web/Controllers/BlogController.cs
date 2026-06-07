using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Nadixa.Core.Entities;
using Nadixa.Infrastructure.Data;
using Nadixa.Web.Models.ViewModels;

namespace Nadixa.Web.Controllers
{
    public class BlogController : Controller
    {
        private readonly NadixaDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<AppUser> _userManager;
        private readonly string[] _allowedExtension = { ".jpg", ".jpeg", ".png", ".jfif" };


        public BlogController(NadixaDbContext context, IWebHostEnvironment webHostEnvironment, UserManager<AppUser> userManager)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index(int? categoryId)
        {
            var query = _context.Blogs.Include(b => b.BlogCategory).Include(b => b.BlogComments).Include(b => b.AppUser).AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(b => b.BlogCategoryId == categoryId);
            }
            var blogs = await query
                .OrderByDescending(b => b.CreateAt)
                .Select(b => new BlogListViewModel
                {
                    Id = b.Id,
                    Title = b.Title,
                    ImageUrl = b.ImageUrl,
                    Author = b.AppUser.FirstName + " " + b.AppUser.LastName,
                    Category = b.BlogCategory.Name,
                    ShortDescription = b.Content.Length > 100 ? b.Content.Substring(0, 100) : b.Content,
                    Date = b.CreateAt,

                    CommentsCount = b.BlogComments.Count()
                })
                .ToListAsync();
            ViewBag.Categories = await _context.BlogCategories.ToListAsync();
            return View(blogs);
        }


        public async Task<IActionResult> Detail(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (id == null) return NotFound();

            var blog = await _context.Blogs.Include(b => b.BlogCategory).Include(b => b.BlogComments).ThenInclude(c => c.AppUser).FirstOrDefaultAsync(b => b.Id == id);

            if (blog == null) return NotFound();
            var vm = new BlogDetailViewModel
            {
                Blog = blog,
            };
            return View(vm);
        }


        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            var blogViewModel = new BlogCreateViewModel();
            blogViewModel.Categories = _context.BlogCategories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();

            return View(blogViewModel);
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(BlogCreateViewModel blogViewModel)
        {
            var user = await _userManager.GetUserAsync(User);
            if (!ModelState.IsValid)
            {
                blogViewModel.Categories = _context.BlogCategories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList();

                return View(blogViewModel);
            }

            string imagePath = null;
            if (blogViewModel.ImageFile != null)
            {
                string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/blogs");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                string fileName = Guid.NewGuid() + Path.GetExtension(blogViewModel.ImageFile.FileName);
                string filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await blogViewModel.ImageFile.CopyToAsync(stream);
                }

                imagePath = "/images/blogs/" + fileName;
            }

            var blog = new Blog
            {
                Title = blogViewModel.Title,
                Content = blogViewModel.ShortDescription,
                ImageUrl = imagePath,
                CreateAt = DateTime.Now,
                AppUserId = user.Id,
                BlogCategoryId = blogViewModel.BlogCategoryId
            };

            _context.Blogs.Add(blog);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var blogFromDb = await _context.Blogs.FirstOrDefaultAsync(p => p.Id == id);
            if (blogFromDb == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(blogFromDb.ImageUrl))
            {
                var existingFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "Images", Path.GetFileName(blogFromDb.ImageUrl));
                if (System.IO.File.Exists(existingFilePath))
                {
                    System.IO.File.Delete(existingFilePath);
                }
            }
            _context.Blogs.Remove(blogFromDb);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Blog");
        }


        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var blogFromDb = await _context.Blogs.FirstOrDefaultAsync(p => p.Id == id);

            if (blogFromDb == null)
            {
                return NotFound();
            }
            BlogEditViewModel editViewModel = new BlogEditViewModel
            {
                Title = blogFromDb.Title,
                Content = blogFromDb.Content,
                ExistingImageUrl = blogFromDb.ImageUrl,
                BlogCategoryId = blogFromDb.BlogCategoryId, // ✅ مهم جدًا

                Categories = _context.BlogCategories.Select(cat =>
                new SelectListItem
                {
                    Value = cat.Id.ToString(),
                    Text = cat.Name
                }).ToList()
            };

            return View(editViewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(BlogEditViewModel editViewModel)
        {
            if (!ModelState.IsValid)
            {
                editViewModel.Categories = _context.BlogCategories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList();

                return View(editViewModel);
            }

            var blogFromDb = await _context.Blogs.FirstOrDefaultAsync(b => b.Id == editViewModel.Id);

            if (blogFromDb == null) return NotFound();

            if (editViewModel.NewImageFile != null)
            {
                var inputFileExtension = Path.GetExtension(editViewModel.NewImageFile.FileName).ToLower();
                bool isAllowed = _allowedExtension.Contains(inputFileExtension);
                if (!isAllowed)
                {
                    ModelState.AddModelError("", "Invalid Image format. Allowed Format are .jpg, .jpeg, .png, .jfif");

                    editViewModel.Categories = _context.BlogCategories.Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    }).ToList();

                    return View(editViewModel);
                }

                if (!string.IsNullOrEmpty(blogFromDb.ImageUrl))
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", blogFromDb.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);

                }
                string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/blogs");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string fileName = Guid.NewGuid() + inputFileExtension;
                string filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await editViewModel.NewImageFile.CopyToAsync(stream);
                }
                blogFromDb.ImageUrl = "/images/blogs/" + fileName;
            }
            blogFromDb.Title = editViewModel.Title;
            blogFromDb.Content = editViewModel.Content;
            blogFromDb.BlogCategoryId = editViewModel.BlogCategoryId;


            await _context.SaveChangesAsync();
            return RedirectToAction("Detail", new { id = blogFromDb.Id });
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddComment(int blogId, string content)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(content))
                return RedirectToAction("Detail", new { id = blogId });

            var comment = new BlogComment
            {
                Content = content,
                BlogId = blogId,
                AppUserId = user.Id,
                CreatedAt = DateTime.Now
            };
            _context.BlogComments.Add(comment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Detail", new { id = blogId });
        }
    }
}
