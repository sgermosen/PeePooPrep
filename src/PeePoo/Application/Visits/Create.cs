using Application.Core;
using Application.Interfaces;
using Application.Visits;
using AutoMapper;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System.Threading;
using System.Threading.Tasks;

namespace Application.PlaceVisits
{
    public class Create
    {
        public class Command : IRequest<Result<Unit>>
        {
            public VisitDto PlaceVisit { get; set; }

        }
        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.PlaceVisit).SetValidator(new VisitValidator());
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
                var user = await _context.Users
                .FirstOrDefaultAsync(p => p.UserName == _userAccessor.GetUsername(), cancellationToken);

                if (user == null) return null;

                var place = await _context.Places
               .FirstOrDefaultAsync(p => p.Id == request.PlaceVisit.PlaceId, cancellationToken);

                if (place == null) return null;

                var placeVisit = new Visit();
                placeVisit = _mapper.Map<Visit>(request.PlaceVisit);

                if (request.PlaceVisit.File != null && request.PlaceVisit.File.Length > 0)
                {
                    var photoUploadResult = await _photoAccessor.AddPhoto(request.PlaceVisit.File);
                    var photo = new VisitPhoto
                    {
                        Id = photoUploadResult.PublicId,
                        Url = photoUploadResult.Url
                    };

                    placeVisit.Photos.Add(photo);
                }

                _context.Visits.Add(placeVisit);

                var result = await _context.SaveChangesAsync(cancellationToken) > 0;
                if (!result) return Result<Unit>.Failure("Fail to create visit");

                return Result<Unit>.Success(Unit.Value);
            }
        }
    }

}
