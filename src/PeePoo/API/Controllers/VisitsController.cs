using API.DTOs;
using Application.Core;
using Application.Moderation;
using Application.Visits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace API.Controllers
{
    /// <summary>Reviews ("visits") of places.</summary>
    public class VisitsController : BaseApiController
    {
        [AllowAnonymous]
        [HttpGet("visitsFromPlace/{id:guid}")]
        public async Task<IActionResult> GetVisits(Guid id)
        {
            return HandleResult(await Mediator.Send(new List.Query { PlaceId = id }));
        }

        [AllowAnonymous]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetVisit(Guid id)
        {
            return HandleResult(await Mediator.Send(new Details.Query { Id = id }));
        }

        /// <summary>Reviews written by the signed-in user.</summary>
        [HttpGet("mine")]
        public async Task<IActionResult> GetMine()
        {
            return HandleResult(await Mediator.Send(new ListMine.Query()));
        }

        [HttpPost]
        [RequestSizeLimit(PhotoRules.MaxBytes + 1024 * 1024)]
        public async Task<IActionResult> CreateVisit([FromForm] VisitInput visit)
        {
            return HandleResult(await Mediator.Send(new Create.Command { Visit = visit }));
        }

        [Authorize(Policy = "IsVisitOwner")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateVisit(Guid id, [FromBody] VisitEditInput visit)
        {
            return HandleResult(await Mediator.Send(new Edit.Command { Id = id, Visit = visit }));
        }

        [Authorize(Policy = "IsVisitOwner")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteVisit(Guid id)
        {
            return HandleResult(await Mediator.Send(new Delete.Command { Id = id }));
        }

        [HttpPost("{id:guid}/report")]
        public async Task<IActionResult> Report(Guid id, ReportDto dto)
        {
            return HandleResult(await Mediator.Send(new CreateReport.Command
            {
                TargetType = CreateReport.VisitTarget,
                TargetId = id,
                Reason = dto?.Reason
            }));
        }
    }
}
