using Application.Core;
using Application.Interfaces;
using AutoMapper;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Visits
{
    public class Create
    {
        public class Command : IRequest<Result<VisitDto>>
        {
            public VisitInput Visit { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Visit).NotNull().SetValidator(new VisitValidator());
            }
        }

        public class Handler : IRequestHandler<Command, Result<VisitDto>>
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

            public async Task<Result<VisitDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                var input = request.Visit;
                var user = await _context.Users
                    .FirstOrDefaultAsync(p => p.UserName == _userAccessor.GetUsername(), cancellationToken);
                if (user == null) return null;

                var place = await _context.Places
                    .FirstOrDefaultAsync(p => p.Id == input.PlaceId && p.IsAproved, cancellationToken);
                if (place == null) return null;

                if (await _context.Visits.AnyAsync(v => v.PlaceId == place.Id && v.AuthorId == user.Id, cancellationToken))
                    return Result<VisitDto>.Failure("You already reviewed this place. Edit your review instead.");

                var id = input.Id is Guid given && given != Guid.Empty ? given : Guid.NewGuid();
                if (await _context.Visits.AnyAsync(v => v.Id == id, cancellationToken))
                    return Result<VisitDto>.Failure("This review was already posted.");

                var visit = new Visit
                {
                    Id = id,
                    PlaceId = place.Id,
                    Author = user,
                    Title = input.Title.Trim(),
                    Description = input.Description.Trim(),
                    Rating = input.Rating,
                    CreatedAt = DateTime.UtcNow
                };

                if (input.File != null && input.File.Length > 0)
                {
                    var upload = await _photoAccessor.AddPhotoLargeFile(input.File);
                    visit.Photos.Add(new VisitPhoto { Id = upload.PublicId, Url = upload.Url, UserId = user.Id, IsAproved = true });
                }

                _context.Visits.Add(visit);
                if (await _context.SaveChangesAsync(cancellationToken) <= 0)
                    return Result<VisitDto>.Failure("Failed to save the review.");

                var dto = await _context.Visits.Where(v => v.Id == id)
                    .ProjectToDto(_mapper, _userAccessor).FirstAsync(cancellationToken);
                return Result<VisitDto>.Success(dto);
            }
        }
    }
}
