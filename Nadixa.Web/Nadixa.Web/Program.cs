using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nadixa.Application.Helpers;
using Nadixa.Application.Interfaces;
using Nadixa.Application.Interfaces.Excel;
using Nadixa.Core.Entities;
using Nadixa.Core.Interfaces;
using Nadixa.Infrastructure.Data;
using Nadixa.Infrastructure.Repositories;
using Nadixa.Infrastructure.Services;
using Nadixa.Infrastructure.Services.Excel;
using Nadixa.Web.Filters;
using Nadixa.Web.Services;
using System.Threading.Tasks;
using AutoMapper;
namespace Nadixa.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddTransient<EmailSender>();
            builder.Services.AddSession();
            // Add services to the container.
            builder.Services.AddControllersWithViews()
                .AddViewLocalization();

            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[] { "en", "fr", "ar" };
                options.SetDefaultCulture(supportedCultures[0])
                       .AddSupportedCultures(supportedCultures)
                       .AddSupportedUICultures(supportedCultures);
            });

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

            //Repositories
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<ICartRepository, CartRepository>();
            builder.Services.AddScoped<IStockNotificationRepository, StockNotificationRepository>();
            builder.Services.AddScoped<IBlogRepository, BlogRepository>();
            builder.Services.AddScoped<ICouponRepository, CouponRepository>();
            builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IWishlistRepository, WishlistRepository>();


            //Services
            builder.Services.AddScoped<IDashboardService, DashboardService>();
            builder.Services.AddScoped<IExcelService, ExcelService>();
            builder.Services.AddScoped<IExcelHelperService, ExcelHelperService>();
            builder.Services.AddScoped<OrderEmailService>();
            builder.Services.AddScoped<StockNotificationService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IPromotionService, PromotionService>();
            builder.Services.AddScoped<IShippingRuleService, ShippingRuleService>();
            builder.Services.AddScoped<ICouponService, CouponService>();
            builder.Services.AddScoped<ILoyaltyService, LoyaltyService>();
            builder.Services.AddScoped<IBundleDealService, BundleDealService>();
            builder.Services.AddScoped<IPermissionService, PermissionService>();
            builder.Services.AddScoped<IHomeService, HomeService>();
            builder.Services.AddScoped<IFileUploadService, FileUploadService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IOrderEmailService, OrderEmailService>();
            builder.Services.AddScoped<ICartService, CartService>();
            builder.Services.AddScoped<IWishlistService, WishlistService>();
            builder.Services.AddScoped<IProfileService, ProfileService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<ISubCategoryService, SubCategoryService>();
            builder.Services.AddScoped<IBlogService, BlogService>();
            builder.Services.AddScoped<IUserManagementService, UserManagementService>();
            builder.Services.AddScoped<IProductImportExportService, ProductImportExportService>();
            builder.Services.AddScoped<IPermissionManagementService, PermissionManagementService>();
            //builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<EmailSender>();
            builder.Services.AddScoped<IRazorViewRenderer, RazorViewRenderer>();            
            builder.Services.AddScoped<IAuthService, Nadixa.Infrastructure.Services.AuthService>();
            builder.Services.AddScoped<IUserOrderHistoryChecker, UserOrderHistoryChecker>();
            builder.Services.AddScoped<IPricingEngine, PricingEngine>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();            
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddAutoMapper(typeof(MappingProfiles).Assembly);
            

            var app = builder.Build();
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

            app.UseRequestLocalization();   // To find the selected language, this middleware will look for the language in 3 locations: URL, Cookie, and the Request header.

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseSession();
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

