using Application.Core;
using Application.Interfaces;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Photos
{
    /// <summary>Adds a photo to a place. Anyone signed in can contribute one.</summary>
    public class Add
    {
        public class Command : IRequest<Result<PhotoDto>>
        {
            public Guid PlaceId { get; set; }
            public IFormFile File { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.PlaceId).NotEmpty();
                RuleFor(x => x.File).NotNull().WithMessage("A photo file is required.")
                    .Must(PhotoRules.IsAcceptable).WithMessage(PhotoRules.Message);
            }
        }

        public class Handler : IRequestHandler<Command, Result<PhotoDto>>
        {
            private readonly IPhotoAccessor _photoAccessor;
            private readonly IUserAccessor _userAccessor;
            private readonly DataContext _context;

            public Handler(DataContext context, IUserAccessor userAccessor, IPhotoAccessor photoAccessor)
            {
                _context = context;
                _photoAccessor = photoAccessor;
                _userAccessor = userAccessor;
            }

            public async Task<Result<PhotoDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(p => p.UserName == _userAccessor.GetUsername(), cancellationToken);
                if (user == null) return null;

                var place = await _context.Places.Include(p => p.Photos)
                    .FirstOrDefaultAsync(p => p.Id == request.PlaceId && p.IsAproved, cancellationToken);
                if (place == null) return null;

                if (place.Photos.Count >= 20)
                    return Result<PhotoDto>.Failure("This place already has the maximum number of photos.");

                var upload = await _photoAccessor.AddPhoto(request.File);
                var photo = new Photo
                {
                    Id = upload.PublicId,
                    Url = upload.Url,
                    UserId = user.Id,
                    IsMain = !place.Photos.Any(p => p.IsMain),
                    IsAproved = true
                };
                place.Photos.Add(photo);
                await _context.SaveChangesAsync(cancellationToken);

                return Result<PhotoDto>.Success(new PhotoDto { Id = photo.Id, Url = photo.Url, IsMain = photo.IsMain });
            }
        }
    }
}
