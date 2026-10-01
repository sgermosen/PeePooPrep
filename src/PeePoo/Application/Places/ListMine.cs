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

namespace Application.Places
{
    /// <summary>The signed-in user's saved places, or the places they added.</summary>
    public class ListMine
    {
        public class Query : IRequest<Result<List<PlaceDto>>>
        {
            /// <summary>True for places the user added, false for places they saved.</summary>
            public bool Owned { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<PlaceDto>>>
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

            public async Task<Result<List<PlaceDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var username = _userAccessor.GetUsername();
                if (username == null) return null;

                var places = await _context.Places
                    .Where(p => p.Favorites.Any(f => f.User.UserName == username && f.IsOwner == request.Owned))
                    .Where(p => p.IsAproved || request.Owned)
                    .OrderByDescending(p => p.CreatedAt)
                    .ProjectToDto(_mapper, _userAccessor)
                    .ToListAsync(cancellationToken);

                return Result<List<PlaceDto>>.Success(places.Select(p => p.Finish()).ToList());
            }
        }
    }
}
