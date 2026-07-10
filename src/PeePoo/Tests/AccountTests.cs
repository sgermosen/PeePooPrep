using Application.Places;
using Application.Profiles;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class AccountTests : TestBase
    {
        [Fact]
        public async Task DeleteAccount_RemovesUserAndTheirContent_ButKeepsPlaces()
        {
            AddUser("u1", "alice");
            var accessor = new FakeUserAccessor { Username = "alice" };

            var dto = new PlaceDto { Id = Guid.NewGuid(), Name = "Alice's spot", Description = "d", Type = "Unisex" };
            await new Create.Handler(Context, accessor, new FakePhotoAccessor(), Mapper)
                .Handle(new Create.Command { Place = dto }, CancellationToken.None);

            await new Application.PlaceVisits.Create.Handler(Context, accessor, new FakePhotoAccessor(), Mapper)
                .Handle(new Application.PlaceVisits.Create.Command
                {
                    PlaceVisit = new Application.Visits.VisitDto { Id = Guid.NewGuid(), PlaceId = dto.Id, Title = "t", Description = "d", Rating = 5 }
                }, CancellationToken.None);

            Assert.Single(Context.Users);
            Assert.Single(Context.Visits);
            Assert.NotEmpty(Context.FavoritePlaces);

            var result = await new DeleteAccount.Handler(Context, accessor).Handle(new DeleteAccount.Command(), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Empty(Context.Users);
            Assert.Empty(Context.Visits);
            Assert.Empty(Context.FavoritePlaces);
            Assert.Single(Context.Places); // the place stays as community data (now owner-less)
        }
    }
}
