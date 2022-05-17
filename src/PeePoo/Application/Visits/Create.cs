
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

namespace Application.Visits
{
    public class Create
    {
        public class Command : IRequest<Result<VisitDto>>
        {
            public Guid PlaceId { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public int Rating { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Title).NotEmpty();
                RuleFor(x => x.Description).NotEmpty();
                RuleFor(x => x.Rating).NotEmpty();
            }
        }

        public class Handler : IRequestHandler<Command, Result<VisitDto>>
        {
            private readonly IUserAccessor _userAccessor;
            private readonly DataContext _context;
            private readonly IMapper _mapper;
            public Handler(DataContext context, IMapper mapper, IUserAccessor userAccessor)
            {
                _mapper = mapper;
                _context = context;
                _userAccessor = userAccessor;
            }

            public async Task<Result<VisitDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                var place = await _context.Places.FindAsync(request.PlaceId, cancellationToken);

                if (place == null) return null;

                var user = await _context.Users
                    .Include(p => p.Photos)
                    .SingleOrDefaultAsync(x => x.UserName == _userAccessor.GetUsername(), cancellationToken);

                var visit = new PlaceVisit
                {
                    Author = user,
                    Place = place,
                    Title = request.Title,
                    Rating = request.Rating,
                    Description = request.Description
                };

                place.Visits.Add(visit);

                var success = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (success) return Result<VisitDto>.Success(_mapper.Map<VisitDto>(visit));

                return Result<VisitDto>.Failure("Failed to add visit");
            }
        }
    }

}
