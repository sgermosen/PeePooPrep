using Application.Places;
using Application.Visits;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace API.Pages
{
    public class LugarModel : PageModel
    {
        private readonly IMediator _mediator;

        public LugarModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        public PlaceDto Place { get; private set; }
        public List<VisitDto> Reviews { get; private set; } = new();
        public string StructuredData { get; private set; }

        public string DirectionsUrl => string.Create(CultureInfo.InvariantCulture,
            $"https://www.google.com/maps/dir/?api=1&destination={Place.Lat},{Place.Long}&travelmode=walking");
        public string OsmUrl => string.Create(CultureInfo.InvariantCulture,
            $"https://www.openstreetmap.org/?mlat={Place.Lat}&mlon={Place.Long}#map=18/{Place.Lat}/{Place.Long}");

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var result = await _mediator.Send(new Application.Places.Details.Query { Id = id });
            if (result?.Value == null || !result.Value.IsAproved) return NotFound();

            Place = result.Value;
            Reviews = (await _mediator.Send(new Application.Visits.List.Query { PlaceId = id }))?.Value ?? new();
            StructuredData = BuildStructuredData();
            return Page();
        }

        private string BuildStructuredData()
        {
            var data = new Dictionary<string, object>
            {
                ["@context"] = "https://schema.org",
                ["@type"] = "PublicToilet",
                ["name"] = Place.Name,
                ["url"] = $"{Request.Scheme}://{Request.Host}/lugar/{Place.Id}",
                ["geo"] = new Dictionary<string, object> { ["@type"] = "GeoCoordinates", ["latitude"] = Place.Lat, ["longitude"] = Place.Long },
                ["isAccessibleForFree"] = Place.IsFree,
            };
            if (!string.IsNullOrWhiteSpace(Place.Description)) data["description"] = Place.Description;
            if (!string.IsNullOrWhiteSpace(Place.Address)) data["address"] = Place.Address;
            if (!string.IsNullOrWhiteSpace(Place.Image)) data["image"] = Place.Image;
            if (Place.ReviewCount > 0 && Place.AverageRating.HasValue)
                data["aggregateRating"] = new Dictionary<string, object>
                {
                    ["@type"] = "AggregateRating",
                    ["ratingValue"] = Place.AverageRating.Value,
                    ["reviewCount"] = Place.ReviewCount,
                    ["bestRating"] = 5,
                    ["worstRating"] = 1
                };
            return JsonSerializer.Serialize(data);
        }
    }
}
