using Application.Core;
using Application.Interfaces;
using AutoMapper;
using MediatR;
using Persistence;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Places
{
    public class Details
    {
        public class Query : IRequest<Result<PlaceDto>>
        {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<PlaceDto>>
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

            public async Task<Result<PlaceDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var place = await PlaceQueries.GetDtoAsync(_context, _mapper, _userAccessor, request.Id, cancellationToken);
                return place == null ? null : Result<PlaceDto>.Success(place);
            }
        }
    }
}
