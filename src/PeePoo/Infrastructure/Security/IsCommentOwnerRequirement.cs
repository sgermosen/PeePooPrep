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
    public class IsCommentOwnerRequirement : IAuthorizationRequirement
    {
    }
    public class IsCommentOwnerRequirementHandler : AuthorizationHandler<IsCommentOwnerRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DataContext _dbContext;
        public IsCommentOwnerRequirementHandler(IHttpContextAccessor httpContextAccessor, DataContext dbContext)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
        }
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, IsCommentOwnerRequirement requirement)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return;

            var routeId = _httpContextAccessor.HttpContext?.Request.RouteValues.SingleOrDefault(x => x.Key == "id").Value?.ToString();
            if (!Guid.TryParse(routeId, out var visitId)) return;

            var visit = await _dbContext.Visits
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == visitId && x.AuthorId == userId);
            if (visit == null) return;

            context.Succeed(requirement);
        }
    }
}
