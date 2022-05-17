using Application.Places;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace API.Controllers
{
    public class PlacesController : BaseApiController
    {

        [HttpGet]
        public async Task<IActionResult> GetPlaces()
        {
            return HandleResult(await Mediator.Send(new List.Query()));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPlace(Guid id)
        {
            return HandleResult(await Mediator.Send(new Details.Query { Id = id }));

        }

        [HttpPost]
        public async Task<IActionResult> CreatePlace(Place place)
        {
            return HandleResult(await Mediator.Send(new Create.Command { Place = place }));
        }

        [Authorize(Policy = "IsPlaceOwner")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePlace(Guid id, Place place)
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
    }
}