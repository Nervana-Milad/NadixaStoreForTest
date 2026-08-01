using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Nadixa.Application.Helpers;
using Nadixa.Core.Entities;
using Nadixa.Core.Interfaces;
using Nadixa.Infrastructure.Services;
using Nadixa.Infrastructure.Data;
using Nadixa.Infrastructure.Repositories;
using Nadixa.Infrastructure.Services;
using Nadixa.Infrastructure.Services.Excel;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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

      



            builder.Services.AddAutoMapper(typeof(MappingProfiles).Assembly);
;

          
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
                };
            });

            builder.Services.AddAuthorization();
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