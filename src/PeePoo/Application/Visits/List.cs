using Application.Core;
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
            public Handler(DataContext context, IMapper mapper)
            {
                _mapper = mapper;
                _context = context;
            }

            public async Task<Result<List<VisitDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var visits = await _context.Visits
                    .Where(x => x.Place.Id == request.PlaceId)
                    .OrderByDescending(x => x.CreatedAt)
                    .ProjectTo<VisitDto>(_mapper.ConfigurationProvider)
                    .ToListAsync();

                return Result<List<VisitDto>>.Success(visits);
            }
        }
    }
}
