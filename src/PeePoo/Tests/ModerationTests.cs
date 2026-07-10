using Application.Moderation;
using Application.Places;
using Application.Visits;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class ModerationTests : TestBase
    {
        private async Task<Guid> SeedPlaceBy(string username)
        {
            var dto = new PlaceDto { Id = Guid.NewGuid(), Name = "Spot", Description = "d", Type = "Unisex" };
            await new Create.Handler(Context, new FakeUserAccessor { Username = username }, new FakePhotoAccessor(), Mapper)
                .Handle(new Create.Command { Place = dto }, CancellationToken.None);
            return dto.Id;
        }

        [Fact]
        public async Task Report_CreatesReportRow()
        {
            AddUser("u1", "alice");
            var placeId = await SeedPlaceBy("alice");

            var result = await new CreateReport.Handler(Context, new FakeUserAccessor { Username = "alice" })
                .Handle(new CreateReport.Command { TargetType = "Place", TargetId = placeId, Reason = "Spam" }, CancellationToken.None);

            Assert.True(result.IsSuccess);
            var report = Assert.Single(Context.Reports);
            Assert.Equal("Place", report.TargetType);
            Assert.Equal(placeId, report.TargetId);
            Assert.Equal("u1", report.ReporterId);
            Assert.False(report.Resolved);
        }

        [Fact]
        public async Task BlockedUsersReviews_AreHiddenFromLists()
        {
            AddUser("u1", "alice");
            AddUser("u2", "bob");
            var placeId = await SeedPlaceBy("alice");

            await new Application.PlaceVisits.Create.Handler(Context, new FakeUserAccessor { Username = "bob" }, new FakePhotoAccessor(), Mapper)
                .Handle(new Application.PlaceVisits.Create.Command
                {
                    PlaceVisit = new VisitDto { Id = Guid.NewGuid(), PlaceId = placeId, Title = "t", Description = "rude", Rating = 1 }
                }, CancellationToken.None);

            var aliceAccessor = new FakeUserAccessor { Username = "alice" };

            var before = await new Application.Visits.List.Handler(Context, Mapper, aliceAccessor)
                .Handle(new Application.Visits.List.Query { PlaceId = placeId }, CancellationToken.None);
            Assert.Single(before.Value);

            await new BlockUser.Handler(Context, aliceAccessor)
                .Handle(new BlockUser.Command { Username = "bob" }, CancellationToken.None);

            var after = await new Application.Visits.List.Handler(Context, Mapper, aliceAccessor)
                .Handle(new Application.Visits.List.Query { PlaceId = placeId }, CancellationToken.None);
            Assert.Empty(after.Value);
        }
    }
}
