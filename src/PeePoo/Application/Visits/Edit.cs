using Application.Core;
using Application.PlaceVisits;
using AutoMapper;
using FluentValidation;
using MediatR;
using Persistence;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Visits
{
    public class Edit
    {
        public class Command : IRequest<Result<Unit>>
        {
            public VisitDto Visit { get; set; }
        }
        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Visit).SetValidator(new VisitValidator());
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
                var visits = await _context.Visits.FindAsync(request.Visit.Id, cancellationToken);

                if (visits == null) return null;

                _mapper.Map(request.Visit, visits);
                var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!result) return Result<Unit>.Failure("Fail to update visit");

                return Result<Unit>.Success(Unit.Value);
            }
        }
    }

}
