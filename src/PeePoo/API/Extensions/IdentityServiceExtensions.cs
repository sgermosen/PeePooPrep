using API.Services;
using Domain;
using Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Persistence;
using System;
using System.Text;
using System.Threading.Tasks;
namespace API.Extensions
{
    public static class IdentityServiceExtensions
    {

        public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddIdentityCore<ApplicationUser>(opt =>
            {
                opt.Password.RequireNonAlphanumeric = false;
                opt.Password.RequiredLength = 8;
                opt.Password.RequireDigit = true;
                opt.Password.RequireLowercase = true;
                opt.Password.RequireUppercase = true;

                opt.User.RequireUniqueEmail = true;

                opt.Lockout.AllowedForNewUsers = true;
                opt.Lockout.MaxFailedAccessAttempts = 5;
                opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            })
             .AddEntityFrameworkStores<DataContext>()
             .AddSignInManager<SignInManager<ApplicationUser>>()
             .AddDefaultTokenProviders();

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));
            var issuer = config["Jwt:Issuer"] ?? "PeePooApi";
            var audience = config["Jwt:Audience"] ?? "PeePooClient";

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
             .AddJwtBearer(opt =>
             {
                 opt.TokenValidationParameters = new TokenValidationParameters
                 {
                     ValidateIssuerSigningKey = true,
                     IssuerSigningKey = key,
                     ValidateIssuer = true,
                     ValidIssuer = issuer,
                     ValidateAudience = true,
                     ValidAudience = audience,
                     ValidateLifetime = true,
                     ClockSkew = TimeSpan.FromMinutes(1),
                 };
             });
            services.AddAuthorization(opt =>
            {
                opt.AddPolicy("IsPlaceOwner", policy =>
                {
                    policy.Requirements.Add(new IsOwnerRequirement());
                });
                opt.AddPolicy("IsVisitOwner", policy =>
                {
                    policy.Requirements.Add(new IsCommentOwnerRequirement());
                });
            });
            services.AddTransient<IAuthorizationHandler, IsOwnerRequirementHandler>();
            services.AddTransient<IAuthorizationHandler, IsCommentOwnerRequirementHandler>();
            services.AddScoped<TokenService>();

            return services;
        }

    }
}
