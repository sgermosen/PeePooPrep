using Application.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Places
{
    public class List
    {
        public class Query : IRequest<Result<List<PlaceDto>>> { }

        public class Handler : IRequestHandler<Query, Result<List<PlaceDto>>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;
            public Handler(DataContext context, IMapper mapper)
            {
                _mapper = mapper;
                _context = context;
            }

            public async Task<Result<List<PlaceDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var places = await _context.Places
                .ProjectTo<PlaceDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

                return Result<List<PlaceDto>>.Success(places);
            }
        }
    }
}
