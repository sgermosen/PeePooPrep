using Application.Places;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class PlacesTests : TestBase
    {
        private PlaceDto NewPlaceDto() => new PlaceDto
        {
            Id = Guid.NewGuid(),
            Name = "Cafe Central WC",
            Description = "Bright and clean",
            Type = "Unisex",
            Rating = 5,
            Long = 18.48,
            Lat = -69.9
        };

        [Fact]
        public async Task Create_MakesCallerTheOwner_AndStampsCreatedAt()
        {
            AddUser("u1", "alice");
            var handler = new Create.Handler(Context, new FakeUserAccessor { Username = "alice" }, new FakePhotoAccessor(), Mapper);

            var dto = NewPlaceDto();
            var result = await handler.Handle(new Create.Command { Place = dto }, CancellationToken.None);

            Assert.True(result.IsSuccess);
            var place = Context.Places.Include(p => p.Favorites).Single();
            var owner = Assert.Single(place.Favorites);
            Assert.Equal("u1", owner.UserId);
            Assert.True(owner.IsOwner);
            Assert.True(place.IsAproved);
            Assert.True((DateTime.UtcNow - place.CreatedAt).TotalMinutes < 1);
        }

        [Fact]
        public async Task Favorite_DoesNotGrantOwnership()
        {
            AddUser("u1", "alice");
            AddUser("u2", "bob");

            var dto = NewPlaceDto();
            await new Create.Handler(Context, new FakeUserAccessor { Username = "alice" }, new FakePhotoAccessor(), Mapper)
                .Handle(new Create.Command { Place = dto }, CancellationToken.None);

            var favHandler = new UpdateFavorite.Handler(Context, new FakeUserAccessor { Username = "bob" });
            await favHandler.Handle(new UpdateFavorite.Command { Id = dto.Id }, CancellationToken.None);

            var bobFavorite = Context.FavoritePlaces.Single(f => f.UserId == "u2" && f.PlaceId == dto.Id);
            Assert.False(bobFavorite.IsOwner);
        }

        [Fact]
        public async Task List_ReturnsPlaces_WithResolvedOwnerUsername()
        {
            AddUser("u1", "alice");
            var dto = NewPlaceDto();
            await new Create.Handler(Context, new FakeUserAccessor { Username = "alice" }, new FakePhotoAccessor(), Mapper)
                .Handle(new Create.Command { Place = dto }, CancellationToken.None);

            var result = await new List.Handler(Context, Mapper).Handle(new List.Query(), CancellationToken.None);

            Assert.True(result.IsSuccess);
            var place = Assert.Single(result.Value);
            Assert.Equal("alice", place.OwnerUsername);
        }
    }
}
