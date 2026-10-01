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
    /// <summary>Saves or un-saves a place for the signed-in user. The person who added a place always keeps it.</summary>
    public class ToggleFavorite
    {
        public class Command : IRequest<Result<FavoriteState>>
        {
            public Guid Id { get; set; }
        }

        public class FavoriteState
        {
            public bool IsFavorite { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<FavoriteState>>
        {
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;

            public Handler(DataContext context, IUserAccessor userAccessor)
            {
                _userAccessor = userAccessor;
                _context = context;
            }

            public async Task<Result<FavoriteState>> Handle(Command request, CancellationToken cancellationToken)
            {
                var user = await _context.Users.SingleOrDefaultAsync(x => x.UserName == _userAccessor.GetUsername(), cancellationToken);
                if (user == null) return null;

                var place = await _context.Places.Where(p => p.IsAproved).VisibleTo(_userAccessor)
                    .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
                if (place == null) return null;

                var existing = await _context.FavoritePlaces
                    .FirstOrDefaultAsync(f => f.PlaceId == request.Id && f.UserId == user.Id, cancellationToken);

                bool isFavorite;
                if (existing == null)
                {
                    _context.FavoritePlaces.Add(new FavoritePlace { PlaceId = place.Id, UserId = user.Id, IsOwner = false });
                    isFavorite = true;
                }
                else if (existing.IsOwner)
                {
                    return Result<FavoriteState>.Success(new FavoriteState { IsFavorite = true });
                }
                else
                {
                    _context.FavoritePlaces.Remove(existing);
                    isFavorite = false;
                }

                await _context.SaveChangesAsync(cancellationToken);
                return Result<FavoriteState>.Success(new FavoriteState { IsFavorite = isFavorite });
            }
        }
    }
}
