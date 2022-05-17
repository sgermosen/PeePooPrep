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
    public class SetMain
    {
        public class Command : IRequest<Result<Unit>>
        {
            public string Id { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;

            public Handler(DataContext context,  IUserAccessor userAccessor)
            {
                _userAccessor = userAccessor;
                _context = context;
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                var user = _context.Users.Include(p => p.Photos)
                    .FirstOrDefault(p => p.UserName == _userAccessor.GetUsername());

                if (user == null) return null;

                var photo = user.Photos.FirstOrDefault(p => p.Id == request.Id);

                if (photo == null) return null;

                if (photo.IsMain)
                    return Result<Unit>.Failure("You cannot set your main photo to the same photo");

                var currentMainPhoto = user.Photos.FirstOrDefault(p => p.IsMain);

                if (currentMainPhoto != null) currentMainPhoto.IsMain = false;

                photo.IsMain = true;

                var success = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (success) return Result<Unit>.Success(Unit.Value);

                return Result<Unit>.Failure("Problem setting photo to main");

            }
        }
    }
}