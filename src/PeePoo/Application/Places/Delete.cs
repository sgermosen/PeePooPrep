using Application.Core;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Places
{
    public class Delete
    {
        public class Command : IRequest<Result<Unit>>
        {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;
            private readonly IPhotoAccessor _photoAccessor;
            private readonly ILogger<Handler> _logger;

            public Handler(DataContext context, IPhotoAccessor photoAccessor, ILogger<Handler> logger)
            {
                _context = context;
                _photoAccessor = photoAccessor;
                _logger = logger;
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                var place = await _context.Places
                    .Include(p => p.Photos)
                    .Include(p => p.Visits).ThenInclude(v => v.Photos)
                    .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
                if (place == null) return null;

                var photoIds = place.Photos.Select(p => p.Id)
                    .Concat(place.Visits.SelectMany(v => v.Photos).Select(p => p.Id))
                    .ToList();

                var visitIds = place.Visits.Select(v => v.Id).ToList();
                var reports = await _context.Reports
                    .Where(r => !r.Resolved && (r.TargetId == place.Id || visitIds.Contains(r.TargetId)))
                    .ToListAsync(cancellationToken);
                reports.ForEach(r => r.Resolved = true);

                _context.Photos.RemoveRange(place.Photos);
                _context.Remove(place);
                await _context.SaveChangesAsync(cancellationToken);

                await PhotoCleanup.DeleteAsync(_photoAccessor, photoIds, _logger);
                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}
