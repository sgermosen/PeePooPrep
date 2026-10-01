using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Moderation
{
    /// <summary>
    /// Locks a user out for good, invalidates their sessions and hides their reviews.
    /// </summary>
    public class BanUser
    {
        public class Command : IRequest<Result<Unit>>
        {
            public string Username { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == request.Username, cancellationToken);
                if (user == null) return null;

                var isAdmin = await (from ur in _context.UserRoles
                                     join r in _context.Roles on ur.RoleId equals r.Id
                                     where ur.UserId == user.Id && r.Name == "Admin"
                                     select ur).AnyAsync(cancellationToken);
                if (isAdmin) return Result<Unit>.Failure("Admins can't be banned.");

                user.LockoutEnabled = true;
                user.LockoutEnd = DateTimeOffset.MaxValue;
                user.SecurityStamp = Guid.NewGuid().ToString("N");

                var visits = await _context.Visits.Where(v => v.AuthorId == user.Id).ToListAsync(cancellationToken);
                visits.ForEach(v => v.IsHidden = true);

                await _context.SaveChangesAsync(cancellationToken);
                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}
