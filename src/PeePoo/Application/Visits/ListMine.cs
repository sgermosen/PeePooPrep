using Application.Core;
using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Visits
{
    public class ListMine
    {
        public class Query : IRequest<Result<List<VisitDto>>>
        {
        }

        public class Handler : IRequestHandler<Query, Result<List<VisitDto>>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;
            private readonly IUserAccessor _userAccessor;

            public Handler(DataContext context, IMapper mapper, IUserAccessor userAccessor)
            {
                _context = context;
                _mapper = mapper;
                _userAccessor = userAccessor;
            }

            public async Task<Result<List<VisitDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var username = _userAccessor.GetUsername();
                if (username == null) return null;

                var visits = await _context.Visits
                    .Where(v => v.Author.UserName == username)
                    .OrderByDescending(v => v.CreatedAt)
                    .ProjectToDto(_mapper, _userAccessor)
                    .ToListAsync(cancellationToken);
                return Result<List<VisitDto>>.Success(visits);
            }
        }
    }
}
