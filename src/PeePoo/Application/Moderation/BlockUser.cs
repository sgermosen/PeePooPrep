using Application.Core;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Moderation
{
    public class BlockUser
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
                var blocker = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserName == _userAccessor.GetUsername(), cancellationToken);
                if (blocker == null) return null;

                var blocked = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserName == request.Username, cancellationToken);
                if (blocked == null) return null;

                if (blocker.Id == blocked.Id)
                    return Result<Unit>.Failure("You cannot block yourself");

                var exists = await _context.UserBlocks
                    .AnyAsync(x => x.BlockerId == blocker.Id && x.BlockedId == blocked.Id, cancellationToken);
                if (exists) return Result<Unit>.Success(Unit.Value);

                _context.UserBlocks.Add(new UserBlock
                {
                    BlockerId = blocker.Id,
                    BlockedId = blocked.Id,
                    CreatedAt = DateTime.UtcNow
                });

                var result = await _context.SaveChangesAsync(cancellationToken) > 0;
                return result ? Result<Unit>.Success(Unit.Value) : Result<Unit>.Failure("Problem blocking the user");
            }
        }
    }
}
