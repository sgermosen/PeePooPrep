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

        private async Task CreatePlaceAt(string name, double lat, double lng, bool babyChanger = false)
        {
            var dto = new PlaceDto
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = "d",
                Type = "Unisex",
                Lat = lat,
                Long = lng,
                HaveBabyChanger = babyChanger
            };
            await new Create.Handler(Context, new FakeUserAccessor { Username = "alice" }, new FakePhotoAccessor(), Mapper)
                .Handle(new Create.Command { Place = dto }, CancellationToken.None);
        }

        [Fact]
        public async Task List_NearLocation_ReturnsWithinRadius_SortedByDistance()
        {
            AddUser("u1", "alice");
            await CreatePlaceAt("Near", 18.4861, -69.9312);
            await CreatePlaceAt("AlsoNear", 18.4900, -69.9300);
            await CreatePlaceAt("Far", 25.7617, -80.1918);

            var result = await new List.Handler(Context, Mapper).Handle(
                new List.Query { Lat = 18.486, Long = -69.931, RadiusKm = 5 }, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Count);
            Assert.Equal("Near", result.Value[0].Name);
            Assert.DoesNotContain(result.Value, p => p.Name == "Far");
            Assert.True(result.Value[0].DistanceKm <= result.Value[1].DistanceKm);
            Assert.NotNull(result.Value[0].DistanceKm);
        }

        [Fact]
        public async Task List_FilterByBabyChanger_ReturnsOnlyMatching()
        {
            AddUser("u1", "alice");
            await CreatePlaceAt("WithChanger", 18.4861, -69.9312, babyChanger: true);
            await CreatePlaceAt("WithoutChanger", 18.4862, -69.9313, babyChanger: false);

            var result = await new List.Handler(Context, Mapper).Handle(
                new List.Query { BabyChanger = true }, CancellationToken.None);

            var place = Assert.Single(result.Value);
            Assert.Equal("WithChanger", place.Name);
        }

        [Fact]
        public async Task Verify_StampsLastVerifiedAt()
        {
            AddUser("u1", "alice");
            var dto = NewPlaceDto();
            await new Create.Handler(Context, new FakeUserAccessor { Username = "alice" }, new FakePhotoAccessor(), Mapper)
                .Handle(new Create.Command { Place = dto }, CancellationToken.None);

            var before = Context.Places.Single();
            Assert.Null(before.LastVerifiedAt);

            var result = await new Verify.Handler(Context).Handle(new Verify.Command { Id = dto.Id }, CancellationToken.None);

            Assert.True(result.IsSuccess);
            var after = Context.Places.Single();
            Assert.NotNull(after.LastVerifiedAt);
            Assert.True((DateTime.UtcNow - after.LastVerifiedAt!.Value).TotalMinutes < 1);
        }
    }
}
