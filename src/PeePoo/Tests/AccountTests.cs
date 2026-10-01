using Application.Profiles;
using Microsoft.Extensions.Logging.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class AccountTests : TestBase
    {
        [Fact]
        public async Task DeleteAccount_RemovesUserContentAndPhotos_ButKeepsPlaces()
        {
            AddUser("u1", "alice");
            var input = NewPlace();
            input.File = FakePhotoAccessor.Image();
            var placeId = await CreatePlaceAs("alice", input);
            await ReviewAs("alice", placeId, file: FakePhotoAccessor.Image());
            var photoIds = Context.Photos.Select(p => p.Id).Concat(Context.VisitPhotos.Select(p => p.Id)).ToList();
            var photos = new FakePhotoAccessor();

            var result = await new DeleteAccount.Handler(Context, As("alice"), photos, NullLogger<DeleteAccount.Handler>.Instance)
                .Handle(new DeleteAccount.Command(), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Empty(Context.Users);
            Assert.Empty(Context.Visits);
            Assert.Empty(Context.Photos);
            Assert.Empty(Context.FavoritePlaces);
            Assert.Single(Context.Places); // the place stays as community data (now owner-less)
            Assert.Equal(photoIds.OrderBy(x => x), photos.Deleted.OrderBy(x => x));
        }

        [Fact]
        public async Task Profile_IncludesContributionCounts()
        {
            AddUser("u1", "alice");
            var placeId = await CreatePlaceAs("alice");
            await ReviewAs("alice", placeId);

            var profile = (await new Details.Handler(Context, Mapper).Handle(new Details.Query { Username = "alice" }, CancellationToken.None)).Value;

            Assert.Equal(1, profile.PlacesCount);
            Assert.Equal(1, profile.ReviewsCount);
        }
    }
}
