using Application.Places;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace API.Controllers
{
    public class PlacesController : BaseApiController
    {

        [HttpGet]
        public async Task<IActionResult> GetPlaces([FromQuery] double? lat, [FromQuery] double? @long,
            [FromQuery] double? radiusKm, [FromQuery] string type, [FromQuery] bool? babyChanger,
            [FromQuery] bool? roomy, [FromQuery] bool? availableOnly)
        {
            return HandleResult(await Mediator.Send(new List.Query
            {
                Lat = lat,
                Long = @long,
                RadiusKm = radiusKm,
                Type = type,
                BabyChanger = babyChanger,
                Roomy = roomy,
                AvailableOnly = availableOnly
            }));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPlace(Guid id)
        {
            return HandleResult(await Mediator.Send(new Details.Query { Id = id }));

        }

        [HttpPost]
        public async Task<IActionResult> CreatePlace([FromForm] PlaceDto place)
        {
            return HandleResult(await Mediator.Send(new Create.Command { Place = place }));
        }

        [Authorize(Policy = "IsPlaceOwner")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePlace(Guid id, PlaceDto place)
        {
            place.Id = id;
            return HandleResult(await Mediator.Send(new Edit.Command { Place = place }));
        }

        [Authorize(Policy = "IsPlaceOwner")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlace(Guid id)
        {
            return HandleResult(await Mediator.Send(new Delete.Command { Id = id }));
        }

        [HttpPost("{id}/favorite")]
        public async Task<IActionResult> Favorite(Guid id)
        {
            return HandleResult(await Mediator.Send(new UpdateFavorite.Command { Id = id }));
        }

        [HttpPost("{id}/verify")]
        public async Task<IActionResult> Verify(Guid id)
        {
            return HandleResult(await Mediator.Send(new Verify.Command { Id = id }));
        }
    }
}
