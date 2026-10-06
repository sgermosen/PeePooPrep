using Application.Visits;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class VisitsTests : TestBase
    {
        [Fact]
        public async Task Create_AssignsAuthor_AndStampsCreatedAt()
        {
            AddUser("u1", "alice");
            AddUser("u2", "bob");
            var placeId = await CreatePlaceAs("alice");

            await ReviewAs("bob", placeId);

            var visit = Context.Visits.Single();
            Assert.Equal("u2", visit.AuthorId);
            Assert.True((DateTime.UtcNow - visit.CreatedAt).TotalMinutes < 1);
        }

        [Fact]
        public async Task Create_ReturnsNullForMissingPlace()
        {
            AddUser("u1", "alice");
            var result = await new Create.Handler(Context, As("alice"), new FakePhotoAccessor(), Mapper)
                .Handle(new Create.Command
                {
                    Visit = new VisitInput { PlaceId = Guid.NewGuid(), Title = "Ghost", Description = "No such place", Rating = 3 }
                }, CancellationToken.None);

            Assert.Null(result);
            Assert.Empty(Context.Visits);
        }

        [Fact]
        public async Task Create_AllowsOneReviewPerPersonAndPlace()
        {
            AddUser("u1", "alice");
            var placeId = await CreatePlaceAs("alice");
            await ReviewAs("alice", placeId);

            var second = await new Create.Handler(Context, As("alice"), new FakePhotoAccessor(), Mapper)
                .Handle(new Create.Command
                {
                    Visit = new VisitInput { PlaceId = placeId, Title = "Again", Description = "Again", Rating = 1 }
                }, CancellationToken.None);

            Assert.False(second.IsSuccess);
            Assert.Single(Context.Visits);
        }

        [Theory]
        [InlineData(0, false)]
        [InlineData(6, false)]
        [InlineData(3, true)]
        public void Validator_RequiresRatingBetweenOneAndFive(int rating, bool valid)
        {
            var input = new VisitInput { PlaceId = Guid.NewGuid(), Title = "t", Description = "d", Rating = rating };
            Assert.Equal(valid, new VisitValidator().Validate(input).IsValid);
        }

        [Fact]
        public async Task Edit_ChangesTextAndRatingOnly()
        {
            AddUser("u1", "alice");
            var placeId = await CreatePlaceAs("alice");
            var id = await ReviewAs("alice", placeId, rating: 5);

            await new Edit.Handler(Context).Handle(new Edit.Command
            {
                Id = id,
                Visit = new VisitEditInput { Title = "Updated", Description = "Changed my mind", Rating = 2 }
            }, CancellationToken.None);

            var visit = Context.Visits.AsNoTracking().Single();
            Assert.Equal("Updated", visit.Title);
            Assert.Equal(2, visit.Rating);
            Assert.Equal(placeId, visit.PlaceId);
        }

        [Fact]
        public async Task List_MarksCallerReviews_AndSkipsHiddenOnes()
        {
            AddUser("u1", "alice");
            AddUser("u2", "bob");
            var placeId = await CreatePlaceAs("alice");
            await ReviewAs("alice", placeId);
            var bobs = await ReviewAs("bob", placeId);

            var list = (await new List.Handler(Context, Mapper, As("alice")).Handle(new List.Query { PlaceId = placeId }, CancellationToken.None)).Value;
            Assert.Equal(2, list.Count);
            Assert.True(list.Single(v => v.Username == "alice").IsMine);
            Assert.False(list.Single(v => v.Username == "bob").IsMine);

            Context.Visits.Single(v => v.Id == bobs).IsHidden = true;
            Context.SaveChanges();
            var after = (await new List.Handler(Context, Mapper, As(null)).Handle(new List.Query { PlaceId = placeId }, CancellationToken.None)).Value;
            Assert.Equal("alice", Assert.Single(after).Username);
        }

        [Fact]
        public async Task Delete_RemovesStoredPhoto()
        {
            AddUser("u1", "alice");
            var placeId = await CreatePlaceAs("alice");
            var id = await ReviewAs("alice", placeId, file: FakePhotoAccessor.Image());
            var photoId = Context.VisitPhotos.Single().Id;
            var photos = new FakePhotoAccessor();

            await new Delete.Handler(Context, photos, NullLogger<Delete.Handler>.Instance)
                .Handle(new Delete.Command { Id = id }, CancellationToken.None);

            Assert.Empty(Context.Visits);
            Assert.Equal(new[] { photoId }, photos.Deleted);
        }
    }
}
