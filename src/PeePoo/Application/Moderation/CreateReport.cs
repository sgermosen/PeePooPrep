using Application.Core;
using Application.Interfaces;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Moderation
{
    public class CreateReport
    {
        public class Command : IRequest<Result<Unit>>
        {
            public string TargetType { get; set; }
            public Guid TargetId { get; set; }
            public string Reason { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
            }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;

            public Handler(DataContext context, IUserAccessor userAccessor)
            {
                _context = context;
                _userAccessor = userAccessor;
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserName == _userAccessor.GetUsername(), cancellationToken);
                if (user == null) return null;

                _context.Reports.Add(new Report
                {
                    TargetType = request.TargetType,
                    TargetId = request.TargetId,
                    ReporterId = user.Id,
                    Reason = request.Reason,
                    CreatedAt = DateTime.UtcNow,
                    Resolved = false
                });

                var result = await _context.SaveChangesAsync(cancellationToken) > 0;
                return result ? Result<Unit>.Success(Unit.Value) : Result<Unit>.Failure("Problem submitting the report");
            }
        }
    }
}
