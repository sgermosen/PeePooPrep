using Application.Core;
using Application.Interfaces;
using AutoMapper;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Places
{
    public class Create
    {
        public class Command : IRequest<Result<PlaceDto>>
        {
            public PlaceInput Place { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Place).NotNull().SetValidator(new PlaceValidator());
            }
        }

        public class Handler : IRequestHandler<Command, Result<PlaceDto>>
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

            public async Task<Result<PlaceDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(p => p.UserName == _userAccessor.GetUsername(), cancellationToken);
                if (user == null) return null;

                var id = request.Place.Id is Guid given && given != Guid.Empty ? given : Guid.NewGuid();
                if (await _context.Places.AnyAsync(p => p.Id == id, cancellationToken))
                    return Result<PlaceDto>.Failure("This place was already added.");

                var place = new Place { Id = id, CreatedAt = DateTime.UtcNow, IsAproved = true };
                place.Apply(request.Place);
                place.Favorites.Add(new FavoritePlace { User = user, Place = place, IsOwner = true });

                if (request.Place.File != null && request.Place.File.Length > 0)
                {
                    var upload = await _photoAccessor.AddPhotoLargeFile(request.Place.File);
                    place.Photos.Add(new Photo { Id = upload.PublicId, Url = upload.Url, IsMain = true, UserId = user.Id, IsAproved = true });
                }

                _context.Places.Add(place);
                if (await _context.SaveChangesAsync(cancellationToken) <= 0)
                    return Result<PlaceDto>.Failure("Failed to save the place.");

                return Result<PlaceDto>.Success(await PlaceQueries.GetDtoAsync(_context, _mapper, _userAccessor, id, cancellationToken));
            }
        }
    }
}
