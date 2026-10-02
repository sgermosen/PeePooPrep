using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Moderation
{
    public class Stats
    {
        public class Summary
        {
            public int Places { get; set; }
            public int HiddenPlaces { get; set; }
            public int Reviews { get; set; }
            public int HiddenReviews { get; set; }
            public int Users { get; set; }
            public int OpenReports { get; set; }
            public int PlacesLast7Days { get; set; }
            public int ReviewsLast7Days { get; set; }
        }

        public class Query : IRequest<Result<Summary>>
        {
        }

        public class Handler : IRequestHandler<Query, Result<Summary>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<Summary>> Handle(Query request, CancellationToken ct)
            {
                var since = DateTime.UtcNow.AddDays(-7);
                return Result<Summary>.Success(new Summary
                {
                    Places = await _context.Places.CountAsync(ct),
                    HiddenPlaces = await _context.Places.CountAsync(p => !p.IsAproved, ct),
                    Reviews = await _context.Visits.CountAsync(ct),
                    HiddenReviews = await _context.Visits.CountAsync(v => v.IsHidden, ct),
                    Users = await _context.Users.CountAsync(ct),
                    OpenReports = await _context.Reports.CountAsync(r => !r.Resolved, ct),
                    PlacesLast7Days = await _context.Places.CountAsync(p => p.CreatedAt >= since, ct),
                    ReviewsLast7Days = await _context.Visits.CountAsync(v => v.CreatedAt >= since, ct)
                });
            }
        }
    }
}
