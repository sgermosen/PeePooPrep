using API.DTOs;
using Application.Core;
using Application.Moderation;
using Application.Places;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace API.Controllers
{
    public class PlacesController : BaseApiController
    {
        /// <summary>Search places. Anyone can browse; signing in is only needed to contribute.</summary>
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetPlaces([FromQuery] double? lat, [FromQuery] double? @long,
            [FromQuery] double? radiusKm, [FromQuery] string q, [FromQuery] string type,
            [FromQuery] bool? babyChanger, [FromQuery] bool? roomy, [FromQuery] bool? accessible,
            [FromQuery] bool? free, [FromQuery] bool? availableOnly, [FromQuery] string sort, [FromQuery] int? limit)
        {
            return HandleResult(await Mediator.Send(new List.Query
            {
                Lat = lat,
                Long = @long,
                RadiusKm = radiusKm,
                Search = q,
                Type = type,
                BabyChanger = babyChanger,
                Roomy = roomy,
                Accessible = accessible,
                Free = free,
                AvailableOnly = availableOnly,
                Sort = sort,
                Limit = limit
            }));
        }

        [AllowAnonymous]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetPlace(Guid id)
        {
            return HandleResult(await Mediator.Send(new Details.Query { Id = id }));
        }

        [AllowAnonymous]
        [HttpGet("{id:guid}/reviews")]
        public async Task<IActionResult> GetReviews(Guid id)
        {
            return HandleResult(await Mediator.Send(new Application.Visits.List.Query { PlaceId = id }));
        }

        [AllowAnonymous]
        [HttpGet("types")]
        public IActionResult GetTypes() => Ok(PlaceTypes.All);

        /// <summary>Places the signed-in user saved.</summary>
        [HttpGet("saved")]
        public async Task<IActionResult> GetSaved()
        {
            return HandleResult(await Mediator.Send(new ListMine.Query { Owned = false }));
        }

        /// <summary>Places the signed-in user added.</summary>
        [HttpGet("mine")]
        public async Task<IActionResult> GetMine()
        {
            return HandleResult(await Mediator.Send(new ListMine.Query { Owned = true }));
        }

        [HttpPost]
        [RequestSizeLimit(PhotoRules.MaxBytes + 1024 * 1024)]
        public async Task<IActionResult> CreatePlace([FromForm] PlaceInput place)
        {
            return HandleResult(await Mediator.Send(new Create.Command { Place = place }));
        }

        [Authorize(Policy = "IsPlaceOwner")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdatePlace(Guid id, [FromBody] PlaceInput place)
        {
            return HandleResult(await Mediator.Send(new Edit.Command { Id = id, Place = place }));
        }

        [Authorize(Policy = "IsPlaceOwner")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeletePlace(Guid id)
        {
            return HandleResult(await Mediator.Send(new Delete.Command { Id = id }));
        }

        /// <summary>Save or un-save a place. Returns { isFavorite }.</summary>
        [HttpPost("{id:guid}/favorite")]
        public async Task<IActionResult> Favorite(Guid id)
        {
            return HandleResult(await Mediator.Send(new ToggleFavorite.Command { Id = id }));
        }

        [Authorize(Policy = "IsPlaceOwner")]
        [HttpPost("{id:guid}/availability")]
        public async Task<IActionResult> SetAvailability(Guid id, [FromBody] AvailabilityDto dto)
        {
            return HandleResult(await Mediator.Send(new SetAvailability.Command { Id = id, IsAvailable = dto.IsAvailable }));
        }

        [HttpPost("{id:guid}/verify")]
        public async Task<IActionResult> Verify(Guid id)
        {
            return HandleResult(await Mediator.Send(new Verify.Command { Id = id }));
        }

        [HttpPost("{id:guid}/report")]
        public async Task<IActionResult> Report(Guid id, ReportDto dto)
        {
            return HandleResult(await Mediator.Send(new CreateReport.Command
            {
                TargetType = CreateReport.PlaceTarget,
                TargetId = id,
                Reason = dto?.Reason
            }));
        }
    }
}
