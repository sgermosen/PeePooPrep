using API.Services;
using Domain;
using Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Persistence;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace API.Extensions
{
    public static class IdentityServiceExtensions
    {
        public const int MinTokenKeyLength = 32;

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
                opt.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789._@+-";

                opt.Lockout.AllowedForNewUsers = true;
                opt.Lockout.MaxFailedAccessAttempts = 5;
                opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            })
             .AddRoles<IdentityRole>()
             .AddEntityFrameworkStores<DataContext>()
             .AddSignInManager<SignInManager<ApplicationUser>>()
             .AddDefaultTokenProviders();

            var tokenKey = config["TokenKey"];
            if (string.IsNullOrWhiteSpace(tokenKey) || tokenKey.Length < MinTokenKeyLength)
                throw new InvalidOperationException(
                    $"TokenKey is missing or shorter than {MinTokenKeyLength} characters. " +
                    "Set it via user-secrets or the TokenKey environment variable (use a long random value).");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));
            var issuer = config["Jwt:Issuer"] ?? "PeePooApi";
            var audience = config["Jwt:Audience"] ?? "PeePooClient";

            services.AddMemoryCache();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
             .AddJwtBearer(opt =>
             {
                 opt.MapInboundClaims = true;
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
                 opt.Events = new JwtBearerEvents { OnTokenValidated = ValidateSessionAsync };
             });

            services.AddAuthorization(opt =>
            {
                opt.AddPolicy("IsPlaceOwner", policy => policy.Requirements.Add(new IsOwnerRequirement()));
                opt.AddPolicy("IsVisitOwner", policy => policy.Requirements.Add(new IsCommentOwnerRequirement()));
            });
            services.AddHttpContextAccessor();
            services.AddTransient<IAuthorizationHandler, IsOwnerRequirementHandler>();
            services.AddTransient<IAuthorizationHandler, IsCommentOwnerRequirementHandler>();
            services.AddScoped<TokenService>();

            return services;
        }

        /// <summary>
        /// Rejects tokens of deleted or banned users and tokens issued before the last password
        /// change (both rotate the security stamp). Short failed-login lockouts deliberately don't
        /// end existing sessions, so strangers can't sign someone out by guessing passwords. Results are cached briefly to avoid a query per request.
        /// </summary>
        private static async Task ValidateSessionAsync(TokenValidatedContext context)
        {
            var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            var stamp = context.Principal?.FindFirstValue(TokenService.StampClaim);
            if (userId == null || stamp == null)
            {
                context.Fail("Session is no longer valid.");
                return;
            }

            var cache = context.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();
            var current = await cache.GetOrCreateAsync("session:" + userId, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
                var db = context.HttpContext.RequestServices.GetRequiredService<DataContext>();
                return await db.Users.AsNoTracking()
                    .Where(u => u.Id == userId)
                    .Select(u => u.SecurityStamp)
                    .FirstOrDefaultAsync();
            });

            if (current == null || current != stamp)
                context.Fail("Session is no longer valid.");
        }

        public static void InvalidateSessionCache(this IMemoryCache cache, string userId) => cache.Remove("session:" + userId);
    }
}
