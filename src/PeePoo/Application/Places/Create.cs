using Application.Core;
using Application.Interfaces;
using AutoMapper;
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
            public PlaceDto Place { get; set; }

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
            private readonly IPhotoAccessor _photoAccessor;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IUserAccessor userAccessor, IPhotoAccessor photoAccessor, IMapper mapper)
            {
                _userAccessor = userAccessor;
                _context = context;
                _photoAccessor = photoAccessor;
                _mapper = mapper;
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                var user = await _context.Users.Include(p => p.Photos)
                .FirstOrDefaultAsync(p => p.UserName == _userAccessor.GetUsername(), cancellationToken);

                if (user == null) return null;

                var place = new Place();
                place = _mapper.Map<Place>(request.Place);
                var favoritee = new FavoritePlace { User = user, Place = place, IsOwner = true };

                place.Favorites.Add(favoritee);
                place.IsAproved = true;

                if (request.Place.File != null && request.Place.File.Length > 0)
                {
                    var photoUploadResult = await _photoAccessor.AddPhoto(request.Place.File);
                    var photo = new Photo
                    {
                        Id = photoUploadResult.PublicId,
                        Url = photoUploadResult.Url,
                        IsMain = true,
                    };

                    place.Photos.Add(photo);
                }

                _context.Places.Add(place);
                var result = await _context.SaveChangesAsync(cancellationToken) > 0;
                if (!result) return Result<Unit>.Failure("Fail to create place");
                return Result<Unit>.Success(Unit.Value);
            }
        }
    }

}
