using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Moderation
{
    /// <summary>
    /// Closes every open report about the same item. With <see cref="Command.Restore"/> the item
    /// becomes visible again (the report was unfounded).
    /// </summary>
    public class ResolveReport
    {
        public class Command : IRequest<Result<Unit>>
        {
            public Guid Id { get; set; }
            public bool Restore { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                var report = await _context.Reports.FindAsync(new object[] { request.Id }, cancellationToken);
                if (report == null) return null;

                var related = await _context.Reports
                    .Where(r => r.TargetId == report.TargetId && !r.Resolved)
                    .ToListAsync(cancellationToken);
                related.ForEach(r => r.Resolved = true);
                report.Resolved = true;

                if (request.Restore)
                {
                    var place = await _context.Places.FindAsync(new object[] { report.TargetId }, cancellationToken);
                    if (place != null) place.IsAproved = true;
                    var visit = await _context.Visits.FindAsync(new object[] { report.TargetId }, cancellationToken);
                    if (visit != null) visit.IsHidden = false;
                }

                await _context.SaveChangesAsync(cancellationToken);
                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}
