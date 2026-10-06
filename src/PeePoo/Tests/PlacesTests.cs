using Application.Places;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class PlacesTests : TestBase
    {
        private List.Handler ListAs(string username = null) => new(Context, Mapper, As(username));

        [Fact]
        public async Task Create_MakesCallerTheOwner_AndApprovesIt()
        {
            AddUser("u1", "alice");
            var id = await CreatePlaceAs("alice");

            var place = Context.Places.Include(p => p.Favorites).Single(p => p.Id == id);
            var owner = Assert.Single(place.Favorites);
            Assert.Equal("u1", owner.UserId);
            Assert.True(owner.IsOwner);
            Assert.True(place.IsAproved);
            Assert.True((DateTime.UtcNow - place.CreatedAt).TotalMinutes < 1);
        }

        [Fact]
        public async Task Create_RejectsAnIdThatAlreadyExists()
        {
            AddUser("u1", "alice");
            var input = NewPlace();
            input.Id = Guid.NewGuid();
            await CreatePlaceAs("alice", input);

            var again = await new Create.Handler(Context, As("alice"), new FakePhotoAccessor(), Mapper)
                .Handle(new Create.Command { Place = input }, CancellationToken.None);

            Assert.False(again.IsSuccess);
            Assert.Single(Context.Places);
        }

        [Fact]
        public async Task Create_NormalizesType_AndMarksAccessibleTypeAsAccessible()
        {
            AddUser("u1", "alice");
            var input = NewPlace();
            input.Type = "accessible";
            var id = await CreatePlaceAs("alice", input);

            var place = Context.Places.Single(p => p.Id == id);
            Assert.Equal("Accessible", place.Type);
            Assert.True(place.IsAccessible);
        }

        [Theory]
        [InlineData(0, 0, "Unisex", false)]
        [InlineData(18.4, -69.9, "Spaceship", false)]
        [InlineData(95, -69.9, "Unisex", false)]
        [InlineData(18.4, -69.9, "Women", true)]
        public void Validator_ChecksLocationAndType(double lat, double lng, string type, bool valid)
        {
            var input = NewPlace(lat: lat, lng: lng);
            input.Type = type;
            Assert.Equal(valid, new PlaceValidator().Validate(input).IsValid);
        }

        [Fact]
        public async Task Rating_IsAverageOfReviews_WhenThereAreAny()
        {
            AddUser("u1", "alice");
            AddUser("u2", "bob");
            AddUser("u3", "carol");
            var id = await CreatePlaceAs("alice"); // submitter rated it 4
            await ReviewAs("bob", id, rating: 5);
            await ReviewAs("carol", id, rating: 2);

            var dto = (await new Details.Handler(Context, Mapper, As(null)).Handle(new Details.Query { Id = id }, CancellationToken.None)).Value;

            Assert.Equal(3.5, dto.AverageRating);
            Assert.Equal(4, dto.Rating);
            Assert.Equal(2, dto.ReviewCount);
        }

        [Fact]
        public async Task Details_ReportsIsFavoriteAndIsOwner_ForTheCaller()
        {
            AddUser("u1", "alice");
            AddUser("u2", "bob");
            var id = await CreatePlaceAs("alice");

            var forAlice = (await new Details.Handler(Context, Mapper, As("alice")).Handle(new Details.Query { Id = id }, CancellationToken.None)).Value;
            var forBob = (await new Details.Handler(Context, Mapper, As("bob")).Handle(new Details.Query { Id = id }, CancellationToken.None)).Value;
            var anonymous = (await new Details.Handler(Context, Mapper, As(null)).Handle(new Details.Query { Id = id }, CancellationToken.None)).Value;

            Assert.True(forAlice.IsOwner);
            Assert.False(forBob.IsOwner);
            Assert.False(forBob.IsFavorite);
            Assert.False(anonymous.IsOwner);
            Assert.Equal("alice", anonymous.OwnerUsername);
        }

        [Fact]
        public async Task HiddenPlaces_AreOnlyVisibleToOwnerAndAdmins()
        {
            AddUser("u1", "alice");
            AddUser("u2", "bob");
            var id = await CreatePlaceAs("alice");
            Context.Places.Single().IsAproved = false;
            Context.SaveChanges();

            Assert.Empty((await ListAs("bob").Handle(new List.Query(), CancellationToken.None)).Value);
            Assert.Null(await new Details.Handler(Context, Mapper, As("bob")).Handle(new Details.Query { Id = id }, CancellationToken.None));
            Assert.NotNull(await new Details.Handler(Context, Mapper, As("alice")).Handle(new Details.Query { Id = id }, CancellationToken.None));
            Assert.NotNull(await new Details.Handler(Context, Mapper, As("mod", admin: true)).Handle(new Details.Query { Id = id }, CancellationToken.None));
        }

        [Fact]
        public async Task Favorite_TogglesForVisitors_AndNeverGrantsOwnership()
        {
            AddUser("u1", "alice");
            AddUser("u2", "bob");
            var id = await CreatePlaceAs("alice");
            var handler = new ToggleFavorite.Handler(Context, As("bob"));

            var saved = await handler.Handle(new ToggleFavorite.Command { Id = id }, CancellationToken.None);
            Assert.True(saved.Value.IsFavorite);
            Assert.False(Context.FavoritePlaces.Single(f => f.UserId == "u2").IsOwner);

            var unsaved = await handler.Handle(new ToggleFavorite.Command { Id = id }, CancellationToken.None);
            Assert.False(unsaved.Value.IsFavorite);
            Assert.DoesNotContain(Context.FavoritePlaces, f => f.UserId == "u2");
        }

        [Fact]
        public async Task Favorite_ByOwner_DoesNotChangeAvailabilityOrOwnership()
        {
            AddUser("u1", "alice");
            var id = await CreatePlaceAs("alice");
            var before = Context.Places.AsNoTracking().Single().IsAvailable;

            var result = await new ToggleFavorite.Handler(Context, As("alice")).Handle(new ToggleFavorite.Command { Id = id }, CancellationToken.None);

            Assert.True(result.Value.IsFavorite);
            Assert.Equal(before, Context.Places.AsNoTracking().Single().IsAvailable);
            Assert.True(Context.FavoritePlaces.Single().IsOwner);
        }

        [Fact]
        public async Task ListMine_SeparatesSavedFromAdded()
        {
            AddUser("u1", "alice");
            AddUser("u2", "bob");
            var alicePlace = await CreatePlaceAs("alice", NewPlace("Alice's"));
            await CreatePlaceAs("bob", NewPlace("Bob's"));
            await new ToggleFavorite.Handler(Context, As("bob")).Handle(new ToggleFavorite.Command { Id = alicePlace }, CancellationToken.None);

            var saved = (await new ListMine.Handler(Context, Mapper, As("bob")).Handle(new ListMine.Query { Owned = false }, CancellationToken.None)).Value;
            var mine = (await new ListMine.Handler(Context, Mapper, As("bob")).Handle(new ListMine.Query { Owned = true }, CancellationToken.None)).Value;

            Assert.Equal("Alice's", Assert.Single(saved).Name);
            Assert.Equal("Bob's", Assert.Single(mine).Name);
        }

        [Fact]
        public async Task List_NearLocation_ReturnsWithinRadius_SortedByDistance()
        {
            AddUser("u1", "alice");
            await CreatePlaceAs("alice", NewPlace("AlsoNear", 18.4900, -69.9300));
            await CreatePlaceAs("alice", NewPlace("Near", 18.4861, -69.9312));
            await CreatePlaceAs("alice", NewPlace("Far", 25.7617, -80.1918));

            var result = await ListAs().Handle(new List.Query { Lat = 18.486, Long = -69.931, RadiusKm = 5 }, CancellationToken.None);

            Assert.Equal(new[] { "Near", "AlsoNear" }, result.Value.Select(p => p.Name));
            Assert.True(result.Value[0].DistanceKm <= result.Value[1].DistanceKm);
        }

        [Fact]
        public async Task List_FiltersAmenities_AndSearchesText()
        {
            AddUser("u1", "alice");
            var accessible = NewPlace("Ramp Cafe");
            accessible.IsAccessible = true;
            accessible.Address = "Calle El Conde";
            await CreatePlaceAs("alice", accessible);
            var paid = NewPlace("Bus Terminal");
            paid.IsFree = false;
            await CreatePlaceAs("alice", paid);

            Assert.Equal("Ramp Cafe", Assert.Single((await ListAs().Handle(new List.Query { Accessible = true }, CancellationToken.None)).Value).Name);
            Assert.Equal("Ramp Cafe", Assert.Single((await ListAs().Handle(new List.Query { Free = true }, CancellationToken.None)).Value).Name);
            Assert.Equal("Ramp Cafe", Assert.Single((await ListAs().Handle(new List.Query { Search = "el conde" }, CancellationToken.None)).Value).Name);
            Assert.Equal("Bus Terminal", Assert.Single((await ListAs().Handle(new List.Query { Search = "TERMINAL" }, CancellationToken.None)).Value).Name);
        }

        [Fact]
        public async Task List_RespectsLimit()
        {
            AddUser("u1", "alice");
            for (var i = 0; i < 5; i++) await CreatePlaceAs("alice", NewPlace("P" + i));

            var result = await ListAs().Handle(new List.Query { Limit = 2 }, CancellationToken.None);

            Assert.Equal(2, result.Value.Count);
        }

        [Fact]
        public async Task Edit_UpdatesOnlyClientFields()
        {
            AddUser("u1", "alice");
            var id = await CreatePlaceAs("alice");
            var input = NewPlace("Renamed");
            input.OpeningHours = "24 horas";

            await new Edit.Handler(Context).Handle(new Edit.Command { Id = id, Place = input }, CancellationToken.None);

            var place = Context.Places.AsNoTracking().Single();
            Assert.Equal("Renamed", place.Name);
            Assert.Equal("24 horas", place.OpeningHours);
            Assert.True(place.IsAproved);
        }

        [Fact]
        public async Task Delete_RemovesStoredPhotosOfPlaceAndReviews()
        {
            AddUser("u1", "alice");
            AddUser("u2", "bob");
            var photos = new FakePhotoAccessor();
            var input = NewPlace();
            input.File = FakePhotoAccessor.Image();
            var id = await CreatePlaceAs("alice", input);
            await ReviewAs("bob", id, file: FakePhotoAccessor.Image());
            var storedIds = Context.Photos.Select(p => p.Id).Concat(Context.VisitPhotos.Select(p => p.Id)).ToList();

            var result = await new Delete.Handler(Context, photos, NullLogger<Delete.Handler>.Instance)
                .Handle(new Delete.Command { Id = id }, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Empty(Context.Places);
            Assert.Empty(Context.Visits);
            Assert.Equal(2, storedIds.Count);
            Assert.Equal(storedIds.OrderBy(x => x), photos.Deleted.OrderBy(x => x));
        }

        [Fact]
        public async Task Verify_StampsLastVerifiedAt()
        {
            AddUser("u1", "alice");
            var id = await CreatePlaceAs("alice");

            var result = await new Verify.Handler(Context, As("alice")).Handle(new Verify.Command { Id = id }, CancellationToken.None);

            Assert.True(result.IsSuccess);
            var after = Context.Places.Single();
            Assert.True((DateTime.UtcNow - after.LastVerifiedAt!.Value).TotalMinutes < 1);
        }
    }
}
