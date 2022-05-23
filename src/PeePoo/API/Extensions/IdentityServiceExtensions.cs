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
            })
             .AddEntityFrameworkStores<DataContext>()
             .AddSignInManager<SignInManager<ApplicationUser>>()
             .AddDefaultTokenProviders();

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
             .AddJwtBearer(opt =>
             {
                 opt.TokenValidationParameters = new TokenValidationParameters
                 {
                     ValidateIssuerSigningKey = true, //COMPARE TO THE SAVED ON SERVER
                     IssuerSigningKey = key,
                     ValidateIssuer = false,
                     ValidateAudience = false,
                 };
                 opt.Events = new JwtBearerEvents
                 {
                     OnMessageReceived = context =>
                     {
                         var accessToken = context.Request.Query["access_token"];
                         var path = context.HttpContext.Request.Path;
                         if (!string.IsNullOrEmpty(accessToken) && (path.StartsWithSegments("/chat")))
                         {
                             context.Token = accessToken;
                         }
                         return Task.CompletedTask;
                     }
                 };
             });
            services.AddAuthorization(opt =>
            {
                opt.AddPolicy("IsPlaceOwner", policy =>
                {
                    policy.Requirements.Add(new IsOwnerRequirement());
                });
                opt.AddPolicy("IsCommentOwner", policy =>
                {
                    policy.Requirements.Add(new IsCommentOwnerRequirement());
                });
            });
            services.AddTransient<IAuthorizationHandler, IsOwnerRequirementHandler>();
            services.AddScoped<TokenService>();

            return services;
        }

    }
}