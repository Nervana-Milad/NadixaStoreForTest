using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Nadixa.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Infrastructure.Data
{
    public class NadixaDbContext : IdentityDbContext<AppUser>
    {
        public NadixaDbContext(DbContextOptions<NadixaDbContext> options) : base(options)
        {
        }

        // تعريف الجداول
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductColor> ProductColors { get; set; }
        public DbSet<Color> Colors { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<WishlistItem> WishlistItems { get; set; }

        public DbSet<Review> Reviews { get; set; }
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<BlogCategory> BlogCategories { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            // ضبط خصائص الأسعار (Decimal) عشان متعملش مشاكل في الداتا بيز
            // بنحدد إن السعر يقبل 18 رقم، منهم 2 عشري
            builder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Product>()
                .Property(p => p.OldPrice)
                .HasColumnType("decimal(18,2)");

            builder.Entity<BlogCategory>().HasData(
                new BlogCategory { Id = 1, Name = "Fashion" },
                new BlogCategory { Id = 2, Name = "Travel" },
                new BlogCategory { Id = 3, Name = "Care" }
            );

            builder.Entity<Blog>().HasData(
                new Blog
                {
                    Id = 1,
                    Title = "Best Bags for Travel",
                    Content = "Discover the best bags for your travel needs, combining style and functionality.",
                    ImageUrl = "/images/blog-01.jpg",
                    CreateAt = new DateTime(2024, 1,1),
                    BlogCategoryId = 2
                },
                new Blog
                {
                    Id = 2,
                    Title = "How to style your bag",
                    Content = "Discover the best bags for your travel needs, combining style and functionality.",
                    ImageUrl = "/images/blog-02.jpg",
                    CreateAt = new DateTime(2024, 1, 1),
                    BlogCategoryId = 2
                }
                );

            // فلترة تلقائية (Global Query Filter)
            // أي استعلام هيرجع بس الحاجات اللي مش ممسوحة (IsDeleted = false)
            builder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
            builder.Entity<Category>().HasQueryFilter(c => !c.IsDeleted);
            builder.Entity<WishlistItem>().HasIndex(w => new { w.WishlistId, w.ProductId }).IsUnique();
        }

    }
}
