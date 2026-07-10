using Application.Core;
using Application.Interfaces;
using Application.Places;
using Infrastructure.Photos;
using Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Persistence;
using System;

namespace API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "PeePoo API", Version = "v1" });
            });

            services.AddDbContext<DataContext>(opt =>
            {
                var provider = config["Database:Provider"] ?? "Sqlite";
                var connectionString = config.GetConnectionString("DefaultConnection");
                if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
                    opt.UseSqlServer(connectionString);
                else
                    opt.UseSqlite(string.IsNullOrWhiteSpace(connectionString) ? "Data Source=peepoo.db" : connectionString);
            });

            services.AddCors(opt =>
            {
                opt.AddPolicy("CorsPolicy", policy =>
                {
                    var origins = config.GetSection("Cors:AllowedOrigins").Get<string[]>()
                                  ?? new[] { "http://localhost:3000" };
                    policy
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials()
                            .WithOrigins(origins);
                });
            });

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(List.Handler).Assembly));
            services.AddAutoMapper(typeof(MappingProfile).Assembly);
            services.AddScoped<IUserAccessor, UserAccessor>();
            services.AddScoped<IPhotoAccessor, PhotoAccessor>();
            services.Configure<CloudinarySettings>(config.GetSection("Cloudinary"));
            services.AddSignalR();
            return services;
        }
    }
}
