using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nadixa.Application.Helpers;
using Nadixa.Core.Entities;
using Nadixa.Core.Interfaces;
using Nadixa.Core.Interfaces.Excel;
using Nadixa.Core.Services;
using Nadixa.Infrastructure.Data;
using Nadixa.Infrastructure.Repositories;
using Nadixa.Infrastructure.Services;
using Nadixa.Infrastructure.Services.Excel;
using System.Reflection;

namespace Nadixa.API

{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Add Services
            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
            });

            // إعدادات Swagger (الشاشة الزرقاء)
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

           

            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<ICartRepository, CartRepository>();
            builder.Services.AddScoped<IStockNotificationRepository, StockNotificationRepository>();
            builder.Services.AddScoped<IBlogRepository, BlogRepository>();
            builder.Services.AddScoped<IHomeService, HomeService>();
            builder.Services.AddScoped<IPromotionService, PromotionService>(); // لو مش مسجل بالفعل
            builder.Services.AddScoped<IProductService, ProductService>();

            builder.Services.AddDbContext<NadixaDbContext>(options =>
            {
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure();
                    });
            });
            // إعدادات Identity (المستخدمين)
            builder.Services.AddIdentity<AppUser, IdentityRole>()
                .AddEntityFrameworkStores<NadixaDbContext>()
                .AddDefaultTokenProviders();

            // تسجيل الـ Repository و UnitOfWork
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


            builder.Services.AddAutoMapper(typeof(MappingProfiles).Assembly);
;


            var app = builder.Build();

            // 2. Configure Pipeline (ترتيب تشغيل الخدمات)
            if (app.Environment.IsDevelopment())
            {
                // تفعيل Swagger في وضع التطوير
                app.UseSwagger();
                app.UseSwaggerUI(); // This requires Swashbuckle.AspNetCore.SwaggerUI NuGet package
            }

            app.UseHttpsRedirection();

            // الترتيب هنا مهم جداً
            app.UseAuthentication(); // لازم Authentication الأول
            app.UseAuthorization();  // وبعدين Authorization



            try
            {
                app.MapControllers();
            }
            catch (ReflectionTypeLoadException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("خطأ في تحميل أحد الملفات أثناء MapControllers():");
                foreach (var loaderEx in ex.LoaderExceptions)
                {
                    Console.WriteLine($"- {loaderEx.Message}");
                }
                Console.ResetColor();
                throw;
            }


            app.Run();
         

        }
    }
}