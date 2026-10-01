using Application.Places;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Pages
{
    public class IndexModel : PageModel
    {
        private readonly DataContext _context;
        private readonly IMediator _mediator;

        public IndexModel(DataContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        public int PlaceCount { get; private set; }
        public int ReviewCount { get; private set; }
        public int AccessibleCount { get; private set; }
        public PlaceDto Example { get; private set; }
        public List<PlaceDto> Latest { get; private set; } = new();

        public async Task OnGetAsync()
        {
            PlaceCount = await _context.Places.CountAsync(p => p.IsAproved);
            ReviewCount = await _context.Visits.CountAsync(v => !v.IsHidden && v.Place.IsAproved);
            AccessibleCount = await _context.Places.CountAsync(p => p.IsAproved && p.IsAccessible);

            var top = await _mediator.Send(new List.Query { Sort = "rating", Limit = 1 });
            Example = top?.Value?.FirstOrDefault(p => p.ReviewCount > 0) ?? top?.Value?.FirstOrDefault();

            Latest = (await _mediator.Send(new List.Query { Sort = "recent", Limit = 4 }))?.Value ?? new();
        }
    }
}
