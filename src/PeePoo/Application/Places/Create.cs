using Application.Core;
using Application.Interfaces;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Places
{
    public class Create
    {
        public class Command : IRequest<Result<Unit>>
        {
            public Place Place { get; set; }
        }
        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Place).SetValidator(new PlaceValidator());
            }
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
                var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == _userAccessor.GetUsername(), cancellationToken);

                var favoritee = new FavoritePlace { User = user, Place = request.Place, IsOwner = true };
                request.Place.Favorites.Add(favoritee);
                request.Place.IsAproved = true;
                _context.Places.Add(request.Place);
                var result = await _context.SaveChangesAsync(cancellationToken) > 0;
                if (!result) return Result<Unit>.Failure("Fail to create place");
                return Result<Unit>.Success(Unit.Value);
            }
        }
    }

}
