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
                    }).ToListAsync(cancellationToken);

                return Result<List<ReportItem>>.Success(reports);
            }
        }
    }
}
