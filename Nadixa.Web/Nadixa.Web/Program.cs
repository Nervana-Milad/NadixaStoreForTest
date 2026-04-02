using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nadixa.Core.Entities;
using Nadixa.Infrastructure.Data;
using Nadixa.Web.Services;
using Nadixa.Web.Filters;

using System.Threading.Tasks;

namespace Nadixa.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddTransient<EmailSender>();
            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<NadixaDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 1;
            }).AddEntityFrameworkStores<NadixaDbContext>();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Auth/Login";
                options.AccessDeniedPath = "/Auth/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
                options.SlidingExpiration = true;
            });

            builder.Services.AddScoped<LoadWishlistFilter>();

            builder.Services.AddControllersWithViews(options =>
            {
                options.Filters.Add<LoadWishlistFilter>();
            });

            var app = builder.Build();


            using (var scope = app.Services.CreateScope())
            {
                var _userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
                var _roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                string adminEmail = "admin@gmail.com";
                string adminPassword = "admin";

                var existingAdminRole = await _roleManager.FindByNameAsync("Admin");

                if(existingAdminRole == null)
                {
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));
                }

                var existingAdminUser = await _userManager.FindByEmailAsync(adminEmail);

                if(existingAdminUser == null)
                {
                    var adminUser = new AppUser { UserName = adminEmail, Email = adminEmail, FirstName = "Admin", LastName = "User" };

                    await _userManager.CreateAsync(adminUser, adminPassword);
                    await _userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }


            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
