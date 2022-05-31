using Application.PlaceVisits;
using Application.Visits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace API.Controllers
{
    public class VisitsController : BaseApiController
    {

        [HttpGet("visitsFromPlace/{id}")]
        public async Task<IActionResult> GetVisits(Guid id)
        {
            return HandleResult(await Mediator.Send(new List.Query {  PlaceId = id }));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVisit(Guid id)
        {
            return HandleResult(await Mediator.Send(new Details.Query { Id = id }));

        }

        [HttpPost]
        public async Task<IActionResult> CreateVisit([FromForm] VisitDto visit)
        {
            return HandleResult(await Mediator.Send(new Create.Command { PlaceVisit = visit }));
        }

        [Authorize(Policy = "IsVisitOwner")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVisit(Guid id, VisitDto visit)
        {
            visit.Id = id;
            return HandleResult(await Mediator.Send(new Edit.Command { Visit = visit }));
        }

        [Authorize(Policy = "IsVisitOwner")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVisit(Guid id)
        {
            return HandleResult(await Mediator.Send(new Delete.Command { Id = id }));
        }


    }
}