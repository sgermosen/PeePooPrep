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
        public const string PlaceTarget = "Place";
        public const string VisitTarget = "Visit";

        /// <summary>Distinct open reports after which content is hidden until a moderator reviews it.</summary>
        public const int AutoHideThreshold = 3;

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
                RuleFor(x => x.TargetType).Must(t => t == PlaceTarget || t == VisitTarget);
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

                Place place = null;
                Visit visit = null;
                if (request.TargetType == PlaceTarget)
                    place = await _context.Places.FindAsync(new object[] { request.TargetId }, cancellationToken);
                else
                    visit = await _context.Visits.FindAsync(new object[] { request.TargetId }, cancellationToken);
                if (place == null && visit == null) return null;

                // One open report per person and item: repeated taps don't pile up.
                var alreadyReported = await _context.Reports.AnyAsync(r =>
                    r.TargetId == request.TargetId && r.ReporterId == user.Id && !r.Resolved, cancellationToken);
                if (alreadyReported) return Result<Unit>.Success(Unit.Value);

                _context.Reports.Add(new Report
                {
                    Id = Guid.NewGuid(),
                    TargetType = request.TargetType,
                    TargetId = request.TargetId,
                    ReporterId = user.Id,
                    Reason = request.Reason.Trim(),
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync(cancellationToken);

                var openReports = await _context.Reports.CountAsync(r => r.TargetId == request.TargetId && !r.Resolved, cancellationToken);
                if (openReports >= AutoHideThreshold)
                {
                    if (place != null) place.IsAproved = false;
                    if (visit != null) visit.IsHidden = true;
                    await _context.SaveChangesAsync(cancellationToken);
                }

                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}
