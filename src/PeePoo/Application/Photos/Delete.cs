using Application.Core;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Photos
{
    /// <summary>Removes a place photo. Allowed for whoever uploaded it, the place's owner, and admins.</summary>
    public class Delete
    {
        public class Command : IRequest<Result<Unit>>
        {
            public string Id { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;
            private readonly IPhotoAccessor _photoAccessor;
            private readonly IUserAccessor _userAccessor;

            public Handler(DataContext context, IPhotoAccessor photoAccessor, IUserAccessor userAccessor)
            {
                _userAccessor = userAccessor;
                _photoAccessor = photoAccessor;
                _context = context;
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                var username = _userAccessor.GetUsername();
                var photo = await _context.Photos.Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
                if (photo == null) return null;

                var isPlaceOwner = await _context.FavoritePlaces
                    .AnyAsync(f => f.PlaceId == photo.PlaceId && f.IsOwner && f.User.UserName == username, cancellationToken);
                if (!_userAccessor.IsAdmin() && !isPlaceOwner && photo.User?.UserName != username)
                    return null;

                await _photoAccessor.DeletePhoto(photo.Id);
                _context.Photos.Remove(photo);

                if (photo.IsMain)
                {
                    var next = await _context.Photos
                        .Where(p => p.PlaceId == photo.PlaceId && p.Id != photo.Id)
                        .FirstOrDefaultAsync(cancellationToken);
                    if (next != null) next.IsMain = true;
                }

                await _context.SaveChangesAsync(cancellationToken);
                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}
