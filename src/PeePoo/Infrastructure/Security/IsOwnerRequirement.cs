using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Infrastructure.Security
{
    public class IsOwnerRequirement : IAuthorizationRequirement
    {
    }
    public class IsOwnerRequirementHandler : AuthorizationHandler<IsOwnerRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DataContext _dbContext;
        public IsOwnerRequirementHandler(IHttpContextAccessor httpContextAccessor, DataContext dbContext)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IsOwnerRequirement requirement)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Task.CompletedTask;
            var activityId = Guid.Parse(_httpContextAccessor.HttpContext?.Request.RouteValues.SingleOrDefault(x => x.Key == "id").Value?.ToString());
            var attendee = _dbContext.FavoritePlaces
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.PlaceId == activityId && x.UserId == userId).Result;
            if (attendee == null) return Task.CompletedTask;

            if (attendee.IsOwner) context.Succeed(requirement);  
            return Task.CompletedTask;
        }
    }
}