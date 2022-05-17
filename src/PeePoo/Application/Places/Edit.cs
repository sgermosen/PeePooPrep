using Application.Core;
using AutoMapper;
using Domain;
using FluentValidation;
using MediatR;
using Persistence;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Places
{
    public class Edit
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
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _mapper = mapper;
                _context = context;
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                var places = await _context.Places.FindAsync(request.Place.Id, cancellationToken);

                if (places == null) return null;

                _mapper.Map(request.Place, places);
                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!result) return Result<Unit>.Failure("Fail to update place");

                return Result<Unit>.Success(Unit.Value);
            }
        }
    }

}
