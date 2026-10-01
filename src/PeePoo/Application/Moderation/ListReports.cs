using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Moderation
{
    public class ListReports
    {
        public class Query : IRequest<Result<List<ReportItem>>>
        {
        }

        public class Handler : IRequestHandler<Query, Result<List<ReportItem>>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<List<ReportItem>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var reports = await (
                    from report in _context.Reports
                    where !report.Resolved
                    join user in _context.Users on report.ReporterId equals user.Id into joined
                    from user in joined.DefaultIfEmpty()
                    orderby report.CreatedAt descending
                    select new ReportItem
                    {
                        Id = report.Id,
                        TargetType = report.TargetType,
                        TargetId = report.TargetId,
                        Reason = report.Reason,
                        CreatedAt = report.CreatedAt,
                        ReporterUsername = user.UserName
                    }).Take(500).ToListAsync(cancellationToken);

                var targetIds = reports.Select(r => r.TargetId).Distinct().ToList();
                var places = await _context.Places.Where(p => targetIds.Contains(p.Id))
                    .Select(p => new
                    {
                        p.Id, p.Name, p.Description, Hidden = !p.IsAproved,
                        Author = p.Favorites.Where(f => f.IsOwner).Select(f => f.User.UserName).FirstOrDefault()
                    })
                    .ToDictionaryAsync(p => p.Id, cancellationToken);
                var visits = await _context.Visits.Where(v => targetIds.Contains(v.Id))
                    .Select(v => new { v.Id, v.Title, v.Description, Hidden = v.IsHidden, Author = v.Author.UserName })
                    .ToDictionaryAsync(v => v.Id, cancellationToken);
                var counts = reports.GroupBy(r => r.TargetId).ToDictionary(g => g.Key, g => g.Count());

                foreach (var report in reports)
                {
                    report.OpenReportsForTarget = counts[report.TargetId];
                    if (places.TryGetValue(report.TargetId, out var place))
                    {
                        report.TargetTitle = place.Name;
                        report.TargetText = place.Description;
                        report.TargetHidden = place.Hidden;
                        report.TargetAuthor = place.Author;
                    }
                    else if (visits.TryGetValue(report.TargetId, out var visit))
                    {
                        report.TargetTitle = visit.Title;
                        report.TargetText = visit.Description;
                        report.TargetHidden = visit.Hidden;
                        report.TargetAuthor = visit.Author;
                    }
                }

                return Result<List<ReportItem>>.Success(reports);
            }
        }
    }
}
