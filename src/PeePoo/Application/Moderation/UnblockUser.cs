using Application.Core;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Moderation
{
    public class UnblockUser
    {
        public class Command : IRequest<Result<Unit>>
        {
            public string Username { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;

            public Handler(DataContext context, IUserAccessor userAccessor)
            {
                _context = context;
                _userAccessor = userAccessor;
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                var me = _userAccessor.GetUsername();
                var block = await _context.UserBlocks.FirstOrDefaultAsync(b =>
                    _context.Users.Any(u => u.Id == b.BlockerId && u.UserName == me) &&
                    _context.Users.Any(u => u.Id == b.BlockedId && u.UserName == request.Username), cancellationToken);
                if (block == null) return null;

                _context.UserBlocks.Remove(block);
                await _context.SaveChangesAsync(cancellationToken);
                return Result<Unit>.Success(Unit.Value);
            }
        }
    }

    public class ListBlocked
    {
        public class Query : IRequest<Result<List<string>>>
        {
        }

        public class Handler : IRequestHandler<Query, Result<List<string>>>
        {
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;

            public Handler(DataContext context, IUserAccessor userAccessor)
            {
                _context = context;
                _userAccessor = userAccessor;
            }

            public async Task<Result<List<string>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var me = _userAccessor.GetUsername();
                var names = await (
                    from b in _context.UserBlocks
                    join blocker in _context.Users on b.BlockerId equals blocker.Id
                    join blocked in _context.Users on b.BlockedId equals blocked.Id
                    where blocker.UserName == me
                    orderby blocked.UserName
                    select blocked.UserName).ToListAsync(cancellationToken);
                return Result<List<string>>.Success(names);
            }
        }
    }
}
