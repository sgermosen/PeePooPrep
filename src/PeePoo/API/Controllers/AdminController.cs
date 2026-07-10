using Application.Moderation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : BaseApiController
    {
        [HttpGet("reports")]
        public async Task<IActionResult> Reports()
        {
            return HandleResult(await Mediator.Send(new ListReports.Query()));
        }

        [HttpPost("reports/{id}/resolve")]
        public async Task<IActionResult> Resolve(Guid id)
        {
            return HandleResult(await Mediator.Send(new ResolveReport.Command { Id = id }));
        }

        [HttpDelete("places/{id}")]
        public async Task<IActionResult> DeletePlace(Guid id)
        {
            return HandleResult(await Mediator.Send(new Application.Places.Delete.Command { Id = id }));
        }

        [HttpDelete("visits/{id}")]
        public async Task<IActionResult> DeleteVisit(Guid id)
        {
            return HandleResult(await Mediator.Send(new Application.Visits.Delete.Command { Id = id }));
        }
    }
}
