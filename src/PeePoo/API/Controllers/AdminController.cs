using API.Extensions;
using Application.Moderation;
using Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : BaseApiController
    {
        [HttpGet("stats")]
        public async Task<IActionResult> Stats()
        {
            return HandleResult(await Mediator.Send(new Stats.Query()));
        }

        [HttpGet("reports")]
        public async Task<IActionResult> Reports()
        {
            return HandleResult(await Mediator.Send(new ListReports.Query()));
        }

        /// <summary>Close a report (and every other open report on the same item). restore=true makes hidden content visible again.</summary>
        [HttpPost("reports/{id:guid}/resolve")]
        public async Task<IActionResult> Resolve(Guid id, [FromQuery] bool restore = false)
        {
            return HandleResult(await Mediator.Send(new ResolveReport.Command { Id = id, Restore = restore }));
        }

        [HttpDelete("places/{id:guid}")]
        public async Task<IActionResult> DeletePlace(Guid id)
        {
            return HandleResult(await Mediator.Send(new Application.Places.Delete.Command { Id = id }));
        }

        [HttpDelete("visits/{id:guid}")]
        public async Task<IActionResult> DeleteVisit(Guid id)
        {
            return HandleResult(await Mediator.Send(new Application.Visits.Delete.Command { Id = id }));
        }

        [HttpPost("users/{username}/ban")]
        public async Task<IActionResult> Ban(string username, [FromServices] UserManager<ApplicationUser> users, [FromServices] IMemoryCache cache)
        {
            var result = await Mediator.Send(new BanUser.Command { Username = username });
            var user = await users.FindByNameAsync(username);
            if (user != null) cache.InvalidateSessionCache(user.Id);
            return HandleResult(result);
        }
    }
}
