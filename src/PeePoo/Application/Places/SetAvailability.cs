using Application.Core;
using MediatR;
using Persistence;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Places
{
    /// <summary>Marks a place open or closed (owner/admin only, enforced by the controller policy).</summary>
    public class SetAvailability
    {
        public class Command : IRequest<Result<Unit>>
        {
            public Guid Id { get; set; }
            public bool IsAvailable { get; set; }
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
                var place = await _context.Places.FindAsync(new object[] { request.Id }, cancellationToken);
                if (place == null) return null;

                place.IsAvailable = request.IsAvailable;
                place.LastVerifiedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}
