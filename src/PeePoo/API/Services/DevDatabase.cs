using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;
using System.Linq;
using System.Threading.Tasks;

namespace API.Services
{
    /// <summary>
    /// Local SQLite databases are created from the model rather than migrated. When the model gains
    /// columns, an old local file is rebuilt (it only ever holds demo data) so `dotnet run` just works.
    /// </summary>
    public static class DevDatabase
    {
        public static async Task EnsureCurrentAsync(DataContext context, ILogger logger)
        {
            await context.Database.EnsureCreatedAsync();
            if (await IsCurrentAsync(context)) return;

            logger.LogWarning("Local SQLite schema is out of date; recreating it with fresh demo data.");
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();
        }

        private static async Task<bool> IsCurrentAsync(DataContext context)
        {
            try
            {
                // Touch the newest columns; this throws if the file predates them.
                await context.Places.Select(p => new { p.OpeningHours, p.IsFree }).FirstOrDefaultAsync();
                await context.Visits.Select(v => v.IsHidden).FirstOrDefaultAsync();
                await context.Users.Select(u => u.CreatedAt).FirstOrDefaultAsync();
                return true;
            }
            catch (SqliteException)
            {
                return false;
            }
        }
    }
}
