using Application.Core;
using Application.Interfaces;
using Domain;
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
    public class Add
    {
        public class Command : IRequest<Result<Photo>>
        {
            public Guid PlaceId { get; set; }

            public IFormFile File { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<Photo>>
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

            public async Task<Result<Photo>> Handle(Command request, CancellationToken cancellationToken)
            {
                var user = await _context.Users.Include(p => p.Photos)
                .FirstOrDefaultAsync(p => p.UserName == _userAccessor.GetUsername(), cancellationToken);

                if (user == null) return null;

                var photoUploadResult = await _photoAccessor.AddPhoto(request.File);
                var photo = new Photo
                {
                    PlaceId = request.PlaceId,
                    Id = photoUploadResult.PublicId,
                    Url = photoUploadResult.Url
                };

                if (!user.Photos.Any(p => p.IsMain))
                    photo.IsMain = true;

                user.Photos.Add(photo);

                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (result)
                    return Result<Photo>.Success(photo);

                return Result<Photo>.Failure("Problem saving photo");

            }
        }
    }
}