using Application.PlaceVisits;
using Application.Visits;
using Domain;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class VisitsTests : TestBase
    {
        private Place SeedPlace()
        {
            var place = new Place
            {
                Id = Guid.NewGuid(),
                Name = "Mall WC",
                Description = "Big",
                Type = "Unisex",
                CreatedAt = DateTime.UtcNow
            };
            Context.Places.Add(place);
            Context.SaveChanges();
            return place;
        }

        [Fact]
        public async Task Create_AssignsAuthor_AndStampsCreatedAt()
        {
            AddUser("u1", "alice");
            var place = SeedPlace();

            var handler = new Create.Handler(Context, new FakeUserAccessor { Username = "alice" }, new FakePhotoAccessor(), Mapper);
            var dto = new VisitDto
            {
                Id = Guid.NewGuid(),
                PlaceId = place.Id,
                Title = "Nice",
                Description = "Clean and roomy",
                Rating = 5
            };

            var result = await handler.Handle(new Create.Command { PlaceVisit = dto }, CancellationToken.None);

            Assert.True(result.IsSuccess);
            var visit = Context.Visits.Single();
            Assert.Equal("u1", visit.AuthorId);
            Assert.True((DateTime.UtcNow - visit.CreatedAt).TotalMinutes < 1);
        }

        [Fact]
        public async Task Create_FailsForMissingPlace()
        {
            AddUser("u1", "alice");
            var handler = new Create.Handler(Context, new FakeUserAccessor { Username = "alice" }, new FakePhotoAccessor(), Mapper);
            var dto = new VisitDto
            {
                Id = Guid.NewGuid(),
                PlaceId = Guid.NewGuid(),
                Title = "Ghost",
                Description = "No such place",
                Rating = 3
            };

            var result = await handler.Handle(new Create.Command { PlaceVisit = dto }, CancellationToken.None);

            Assert.Null(result);
            Assert.Empty(Context.Visits);
        }
    }
}
