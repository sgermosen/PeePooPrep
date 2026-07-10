using Application.Core;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Profiles
{
    public class DeleteAccount
    {
        public class Command : IRequest<Result<Unit>>
        {
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;

            public Handler(DataContext context, IUserAccessor userAccessor)
            {
                _context = context;
                _userAccessor = userAccessor;
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserName == _userAccessor.GetUsername(), cancellationToken);
                if (user == null) return null;

                var visitPhotos = await _context.VisitPhotos.Where(p => p.UserId == user.Id).ToListAsync(cancellationToken);
                _context.VisitPhotos.RemoveRange(visitPhotos);

                var visits = await _context.Visits.Where(v => v.AuthorId == user.Id).ToListAsync(cancellationToken);
                _context.Visits.RemoveRange(visits);

                var photos = await _context.Photos.Where(p => p.UserId == user.Id).ToListAsync(cancellationToken);
                _context.Photos.RemoveRange(photos);

                var favorites = await _context.FavoritePlaces.Where(f => f.UserId == user.Id).ToListAsync(cancellationToken);
                _context.FavoritePlaces.RemoveRange(favorites);

                _context.Users.Remove(user);

                var result = await _context.SaveChangesAsync(cancellationToken) > 0;
                return result ? Result<Unit>.Success(Unit.Value) : Result<Unit>.Failure("Problem deleting the account");
            }
        }
    }
}
