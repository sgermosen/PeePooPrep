using Application.Places;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace API.Pages
{
    public class ExplorarModel : PageModel
    {
        private readonly IMediator _mediator;

        public ExplorarModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        [BindProperty(SupportsGet = true, Name = "q")] public string Search { get; set; }
        [BindProperty(SupportsGet = true, Name = "tipo")] public string Type { get; set; }
        [BindProperty(SupportsGet = true, Name = "accesible")] public bool Accessible { get; set; }
        [BindProperty(SupportsGet = true, Name = "cambiador")] public bool BabyChanger { get; set; }
        [BindProperty(SupportsGet = true, Name = "gratis")] public bool Free { get; set; }
        [BindProperty(SupportsGet = true, Name = "abierto")] public bool Open { get; set; }

        public List<PlaceDto> Places { get; private set; } = new();
        public string MapJson { get; private set; } = "[]";

        public static readonly (string Value, string Label)[] Types =
        {
            ("", "Todos"), ("Unisex", "Mixto"), ("Family", "Familiar"), ("Women", "Mujeres"), ("Men", "Hombres"), ("Accessible", "Adaptado")
        };

        public async Task OnGetAsync()
        {
            var result = await _mediator.Send(new List.Query
            {
                Search = Search,
                Type = Type,
                Accessible = Accessible ? true : null,
                BabyChanger = BabyChanger ? true : null,
                Free = Free ? true : null,
                AvailableOnly = Open ? true : null,
                Sort = "rating",
                Limit = 300
            });
            Places = result?.Value ?? new();
            MapJson = JsonSerializer.Serialize(Places.Select(p => new
            {
                id = p.Id,
                name = p.Name,
                lat = p.Lat,
                lng = p.Long,
                rating = p.ReviewCount > 0 ? p.AverageRating : null,
                open = p.IsAvailable
            }));
        }
    }
}
