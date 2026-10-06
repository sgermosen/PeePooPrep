using Application.Core;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Profiles
{
    /// <summary>
    /// Deletes the signed-in user, their reviews, their photos and favorites.
    /// Places they added stay as anonymous community entries.
    /// </summary>
    public class DeleteAccount
    {
        public class Command : IRequest<Result<Unit>>
        {
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;
            private readonly IPhotoAccessor _photoAccessor;
            private readonly ILogger<Handler> _logger;

            public Handler(DataContext context, IUserAccessor userAccessor, IPhotoAccessor photoAccessor, ILogger<Handler> logger)
            {
                _context = context;
                _userAccessor = userAccessor;
                _photoAccessor = photoAccessor;
                _logger = logger;
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserName == _userAccessor.GetUsername(), cancellationToken);
                if (user == null) return null;

                var visits = await _context.Visits.Include(v => v.Photos)
                    .Where(v => v.AuthorId == user.Id).ToListAsync(cancellationToken);
                var visitPhotos = visits.SelectMany(v => v.Photos).ToList();
                var placePhotos = await _context.Photos.Where(p => p.UserId == user.Id).ToListAsync(cancellationToken);
                var photoIds = visitPhotos.Select(p => p.Id).Concat(placePhotos.Select(p => p.Id)).ToList();

                _context.VisitPhotos.RemoveRange(visitPhotos);
                _context.Visits.RemoveRange(visits);
                _context.Photos.RemoveRange(placePhotos);
                _context.FavoritePlaces.RemoveRange(
                    await _context.FavoritePlaces.Where(f => f.UserId == user.Id).ToListAsync(cancellationToken));
                _context.UserBlocks.RemoveRange(await _context.UserBlocks
                    .Where(b => b.BlockerId == user.Id || b.BlockedId == user.Id).ToListAsync(cancellationToken));
                _context.Reports.RemoveRange(
                    await _context.Reports.Where(r => r.ReporterId == user.Id).ToListAsync(cancellationToken));

                _context.Users.Remove(user);
                await _context.SaveChangesAsync(cancellationToken);

                await PhotoCleanup.DeleteAsync(_photoAccessor, photoIds, _logger);
                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}
