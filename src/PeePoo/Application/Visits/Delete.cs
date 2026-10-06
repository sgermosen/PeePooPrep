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

namespace Application.Visits
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
                var visit = await _context.Visits.Include(v => v.Photos)
                    .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken);
                if (visit == null) return null;

                var photoIds = visit.Photos.Select(p => p.Id).ToList();
                var reports = await _context.Reports.Where(r => !r.Resolved && r.TargetId == visit.Id).ToListAsync(cancellationToken);
                reports.ForEach(r => r.Resolved = true);

                _context.Remove(visit);
                await _context.SaveChangesAsync(cancellationToken);

                await PhotoCleanup.DeleteAsync(_photoAccessor, photoIds, _logger);
                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}
