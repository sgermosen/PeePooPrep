using Application.Core;
using Application.Interfaces;
using AutoMapper;
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
    /// <summary>Reviews for a place, newest first, without hidden reviews or reviews from users the caller blocked.</summary>
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
                var username = _userAccessor.GetUsername();
                var blockedIds = username == null
                    ? new List<string>()
                    : await _context.UserBlocks
                        .Where(b => _context.Users.Any(u => u.Id == b.BlockerId && u.UserName == username))
                        .Select(b => b.BlockedId)
                        .ToListAsync(cancellationToken);

                var visits = await _context.Visits
                    .Where(x => x.PlaceId == request.PlaceId && !x.IsHidden && x.Place.IsAproved && !blockedIds.Contains(x.AuthorId))
                    .OrderByDescending(x => x.CreatedAt)
                    .Take(200)
                    .ProjectToDto(_mapper, _userAccessor)
                    .ToListAsync(cancellationToken);

                return Result<List<VisitDto>>.Success(visits);
            }
        }
    }
}
