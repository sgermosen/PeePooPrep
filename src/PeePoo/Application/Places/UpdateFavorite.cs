using Application.Core;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Places
{
    public class UpdateFavorite
    {

        public class Command : IRequest<Result<Unit>>
        {
            public Guid Id { get; set; }
            public bool IsHost { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;
            public Handler(DataContext context, IUserAccessor userAccessor)
            {
                _userAccessor = userAccessor;
                _context = context;
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                var place = await _context.Places
                .Include(x => x.Favorites).ThenInclude(x => x.User)
                .SingleOrDefaultAsync(x => x.Id == request.Id);
                if (place == null) return null;

                var user = await _context.Users.SingleOrDefaultAsync(x => x.UserName == _userAccessor.GetUsername());
                if (user == null) return null;

                var hostUsername = place.Favorites.FirstOrDefault
                    (x => x.IsOwner)?.User?.UserName;

                var favoritee = place.Favorites.FirstOrDefault(x => x.User.UserName == user.UserName);

                if (favoritee != null && hostUsername == user.UserName)
                    place.IsAvailable = !place.IsAvailable;

                if (favoritee != null && hostUsername != user.UserName)
                    place.Favorites.Remove(favoritee);

                if (favoritee == null)
                {
                    favoritee = new FavoritePlace
                    {
                        User = user,
                        IsOwner = true,
                        Place = place
                    };
                    place.Favorites.Add(favoritee);
                }

                var result = await _context.SaveChangesAsync() > 0;

                return result ? Result<Unit>.Success(Unit.Value) : Result<Unit>.Failure("Problem saving changes");

            }
        }
    }
}
