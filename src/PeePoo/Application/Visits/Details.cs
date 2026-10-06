using Application.Core;
using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Visits
{
    public class Details
    {
        public class Query : IRequest<Result<VisitDto>>
        {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<VisitDto>>
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

            public async Task<Result<VisitDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var username = _userAccessor.GetUsername();
                var isAdmin = _userAccessor.IsAdmin();
                var visit = await _context.Visits
                    .Where(v => v.Id == request.Id && (isAdmin || !v.IsHidden || v.Author.UserName == username))
                    .ProjectToDto(_mapper, _userAccessor)
                    .FirstOrDefaultAsync(cancellationToken);

                return visit == null ? null : Result<VisitDto>.Success(visit);
            }
        }
    }
}
