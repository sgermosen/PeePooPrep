using Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Persistence
{
    public class DataContext : IdentityDbContext<ApplicationUser>
    {
        public DataContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Place> Places { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<PlaceVisit> Visits { get; set; }
        public DbSet<FavoritePlace> FavoritePlaces { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<FavoritePlace>()
                .HasKey(x => new { x.PlaceId, x.UserId });

            builder.Entity<FavoritePlace>()
                .HasOne(a => a.Place)
                .WithMany(u => u.Favorites)
                .HasForeignKey(aa => aa.PlaceId);

            builder.Entity<FavoritePlace>()
                .HasOne(u => u.User)
                .WithMany(a => a.FavoritePlaces)
                .HasForeignKey(aa => aa.UserId);

            builder.Entity<PlaceVisit>()
               .HasOne(a => a.Place)
               .WithMany(c => c.Visits)
               .OnDelete(DeleteBehavior.Cascade);
        }
    }
}