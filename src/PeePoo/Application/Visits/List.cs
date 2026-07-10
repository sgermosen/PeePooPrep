using Application.Core;
using Application.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Visits
{
    public class List
    {
        public class Query : IRequest<Result<List<VisitDto>>>
        {
            public Guid PlaceId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<VisitDto>>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;
            private readonly IUserAccessor _userAccessor;
            public Handler(DataContext context, IMapper mapper, IUserAccessor userAccessor)
            {
                _mapper = mapper;
                _context = context;
                _userAccessor = userAccessor;
            }

            public async Task<Result<List<VisitDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var currentUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserName == _userAccessor.GetUsername(), cancellationToken);

                var blockedIds = currentUser == null
                    ? new List<string>()
                    : await _context.UserBlocks
                        .Where(b => b.BlockerId == currentUser.Id)
                        .Select(b => b.BlockedId)
                        .ToListAsync(cancellationToken);

                var visits = await _context.Visits
                    .Where(x => x.Place.Id == request.PlaceId && !blockedIds.Contains(x.AuthorId))
                    .OrderByDescending(x => x.CreatedAt)
                    .ProjectTo<VisitDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                return Result<List<VisitDto>>.Success(visits);
            }
        }
    }
}
