using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nadixa.Core.Entities;
using Nadixa.Core.Interfaces;
using Nadixa.Infrastructure.Services;
using Nadixa.Infrastructure.Data;
using Nadixa.Infrastructure.Repositories;
using Nadixa.Web.Filters;
using Nadixa.Web.Services;
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
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
                options.SignIn.RequireConfirmedAccount = false;

            }).AddEntityFrameworkStores<NadixaDbContext>().AddDefaultTokenProviders();
            builder.Services.AddAuthentication()
               .AddGoogle(options =>
               {
                   options.ClientId = "361328285969-nuqttpr8ooqv9loj1pdmir4h5mqj2f9b.apps.googleusercontent.com";
                   options.ClientSecret = "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";
               });

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


            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IDashboardService, DashboardService>();

            var app = builder.Build();


            //using (var scope = app.Services.CreateScope())
            //{
            //    var _userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            //    var _roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();


            //    string adminEmail = "admin@gmail.com";
            //    string adminPassword = "admin";

            //    var existingAdminRole = await _roleManager.FindByNameAsync("Admin");

            //    if (existingAdminRole == null)
            //    {
            //        await _roleManager.CreateAsync(new IdentityRole("Admin"));
            //    }

            //    var existingAdminUser = await _userManager.FindByEmailAsync(adminEmail);

            //    if (existingAdminUser == null)
            //    {
            //        var adminUser = new AppUser { UserName = adminEmail, Email = adminEmail, FirstName = "Admin", LastName = "User" };

            //        await _userManager.CreateAsync(adminUser, adminPassword);
            //        await _userManager.AddToRoleAsync(adminUser, "Admin");
            //    }
            //}
            using (var scope = app.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                string adminEmail = "admin@gmail.com";
                string adminPassword = "admin123";

                string adminRoleName = "Admin";

                // 1. Create Role if not exists
                var roleExists = await roleManager.RoleExistsAsync(adminRoleName);
                if (!roleExists)
                {
                    var roleResult = await roleManager.CreateAsync(new IdentityRole(adminRoleName));

                    if (!roleResult.Succeeded)
                        throw new Exception("Failed to create Admin role");
                }

                // 2. Create User if not exists
                var adminUser = await userManager.FindByEmailAsync(adminEmail);

                if (adminUser == null)
                {
                    adminUser = new AppUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        FirstName = "Admin",
                        LastName = "User",
                        EmailConfirmed = true
                    };

                    var userResult = await userManager.CreateAsync(adminUser, adminPassword);

                    if (!userResult.Succeeded)
                        throw new Exception("Failed to create Admin user");

                    // 3. Assign Role after user creation
                    var addToRoleResult = await userManager.AddToRoleAsync(adminUser, adminRoleName);

                    if (!addToRoleResult.Succeeded)
                        throw new Exception("Failed to assign Admin role to user");
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
            app.UseAuthentication();
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
