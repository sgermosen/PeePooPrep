using Application.Moderation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class ModerationTests : TestBase
    {
        private Task<Application.Core.Result<MediatR.Unit>> Report(string username, string type, Guid id, string reason = "Spam") =>
            new CreateReport.Handler(Context, As(username))
                .Handle(new CreateReport.Command { TargetType = type, TargetId = id, Reason = reason }, CancellationToken.None);

        [Fact]
        public async Task Report_CreatesReportRow()
        {
            AddUser("u1", "alice");
            var placeId = await CreatePlaceAs("alice");

            var result = await Report("alice", "Place", placeId);

            Assert.True(result.IsSuccess);
            var report = Assert.Single(Context.Reports);
            Assert.Equal(placeId, report.TargetId);
            Assert.Equal("u1", report.ReporterId);
            Assert.False(report.Resolved);
        }

        [Fact]
        public async Task Report_ForMissingContent_ReturnsNull()
        {
            AddUser("u1", "alice");
            Assert.Null(await Report("alice", "Visit", Guid.NewGuid()));
            Assert.Empty(Context.Reports);
        }

        [Fact]
        public async Task Report_IsCountedOncePerPerson()
        {
            AddUser("u1", "alice");
            var placeId = await CreatePlaceAs("alice");

            await Report("alice", "Place", placeId);
            await Report("alice", "Place", placeId);
            await Report("alice", "Place", placeId);

            Assert.Single(Context.Reports);
            Assert.True(Context.Places.Single().IsAproved);
        }

        [Fact]
        public async Task ThreeDistinctReports_HideContent_AndRestoreBringsItBack()
        {
            AddUser("u1", "alice");
            AddUser("u2", "bob");
            AddUser("u3", "carol");
            AddUser("u4", "dan");
            var placeId = await CreatePlaceAs("alice");
            var reviewId = await ReviewAs("alice", placeId);

            foreach (var who in new[] { "bob", "carol", "dan" })
                await Report(who, "Visit", reviewId);

            Assert.True(Context.Visits.AsNoTracking().Single().IsHidden);

            var reports = (await new ListReports.Handler(Context).Handle(new ListReports.Query(), CancellationToken.None)).Value;
            Assert.Equal(3, reports.Count);
            Assert.All(reports, r => Assert.Equal(3, r.OpenReportsForTarget));
            Assert.All(reports, r => Assert.True(r.TargetHidden));

            await new ResolveReport.Handler(Context).Handle(new ResolveReport.Command { Id = reports[0].Id, Restore = true }, CancellationToken.None);

            Assert.False(Context.Visits.AsNoTracking().Single().IsHidden);
            Assert.All(Context.Reports.AsNoTracking(), r => Assert.True(r.Resolved));
        }

        [Fact]
        public async Task BlockedUsersReviews_AreHiddenFromLists_UntilUnblocked()
        {
            AddUser("u1", "alice");
            AddUser("u2", "bob");
            var placeId = await CreatePlaceAs("alice");
            await ReviewAs("bob", placeId, rating: 1);
            var list = new Application.Visits.List.Handler(Context, Mapper, As("alice"));
            var query = new Application.Visits.List.Query { PlaceId = placeId };

            Assert.Single((await list.Handle(query, CancellationToken.None)).Value);

            await new BlockUser.Handler(Context, As("alice")).Handle(new BlockUser.Command { Username = "bob" }, CancellationToken.None);
            Assert.Empty((await list.Handle(query, CancellationToken.None)).Value);
            Assert.Equal(new[] { "bob" }, (await new ListBlocked.Handler(Context, As("alice")).Handle(new ListBlocked.Query(), CancellationToken.None)).Value);

            await new UnblockUser.Handler(Context, As("alice")).Handle(new UnblockUser.Command { Username = "bob" }, CancellationToken.None);
            Assert.Single((await list.Handle(query, CancellationToken.None)).Value);
        }

        [Fact]
        public async Task Ban_LocksUser_RotatesStamp_AndHidesReviews()
        {
            AddUser("u1", "alice");
            AddUser("u2", "bob");
            var placeId = await CreatePlaceAs("alice");
            await ReviewAs("bob", placeId);
            var oldStamp = Context.Users.AsNoTracking().Single(u => u.Id == "u2").SecurityStamp;

            var result = await new BanUser.Handler(Context).Handle(new BanUser.Command { Username = "bob" }, CancellationToken.None);

            Assert.True(result.IsSuccess);
            var bob = Context.Users.AsNoTracking().Single(u => u.Id == "u2");
            Assert.True(bob.LockoutEnd > DateTimeOffset.UtcNow.AddYears(10));
            Assert.NotEqual(oldStamp, bob.SecurityStamp);
            Assert.True(Context.Visits.AsNoTracking().Single().IsHidden);
        }

        [Fact]
        public async Task Stats_CountsContent()
        {
            AddUser("u1", "alice");
            var placeId = await CreatePlaceAs("alice");
            await ReviewAs("alice", placeId);
            await Report("alice", "Place", placeId);

            var stats = (await new Stats.Handler(Context).Handle(new Stats.Query(), CancellationToken.None)).Value;

            Assert.Equal(1, stats.Places);
            Assert.Equal(1, stats.Reviews);
            Assert.Equal(1, stats.Users);
            Assert.Equal(1, stats.OpenReports);
            Assert.Equal(1, stats.PlacesLast7Days);
        }
    }
}
