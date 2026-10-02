using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Integration
{
    public class ApiTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public ApiTests(ApiFactory factory)
        {
            _factory = factory;
        }

        private static async Task<JsonElement> Json(HttpResponseMessage response)
        {
            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return doc.RootElement.Clone();
        }

        [Fact]
        public async Task Guests_CanBrowsePlacesAndReviews()
        {
            var client = _factory.CreateClient();

            var places = await Json(await client.GetAsync("/api/places?lat=18.47&long=-69.93&radiusKm=20"));
            Assert.True(places.GetArrayLength() > 5);
            var first = places[0];
            Assert.True(first.GetProperty("distanceKm").GetDouble() >= 0);

            var id = first.GetProperty("id").GetString();
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/places/{id}")).StatusCode);
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/visits/visitsFromPlace/{id}")).StatusCode);
        }

        [Theory]
        [InlineData("GET", "/api/account")]
        [InlineData("DELETE", "/api/account")]
        [InlineData("GET", "/api/places/saved")]
        [InlineData("POST", "/api/places")]
        [InlineData("GET", "/api/admin/reports")]
        public async Task Guests_CannotUseAccountOrWriteEndpoints(string method, string url)
        {
            var response = await _factory.CreateClient().SendAsync(new HttpRequestMessage(new HttpMethod(method), url));
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task RegularUsers_CannotUseAdminEndpoints()
        {
            var client = await _factory.SignedInClientAsync();
            Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/admin/reports")).StatusCode);
        }

        [Fact]
        public async Task Responses_CarrySecurityHeaders()
        {
            var response = await _factory.CreateClient().GetAsync("/api/places");

            Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
            Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").Single());
            Assert.Contains("frame-ancestors 'none'", response.Headers.GetValues("Content-Security-Policy").Single());
        }

        [Fact]
        public async Task Login_IgnoresEmailCase_AndFailuresReturnAMessage()
        {
            var client = _factory.CreateClient();

            var ok = await client.PostAsJsonAsync("/api/account/login", new { email = "ALFREDO@TEST.COM", password = Persistence.Seed.DemoPassword });
            Assert.Equal(HttpStatusCode.OK, ok.StatusCode);

            var bad = await client.PostAsJsonAsync("/api/account/login", new { email = "alfredo@test.com", password = "wrong" });
            Assert.Equal(HttpStatusCode.Unauthorized, bad.StatusCode);
            Assert.False(string.IsNullOrEmpty((await Json(bad)).GetProperty("message").GetString()));
        }

        [Fact]
        public async Task ValidationErrors_ReturnAReadableMessage()
        {
            var client = await _factory.SignedInClientAsync();
            var form = new MultipartFormDataContent
            {
                { new StringContent("X"), "Name" },
                { new StringContent("Unisex"), "Type" },
                { new StringContent("0"), "Lat" },
                { new StringContent("0"), "Long" },
            };

            var response = await client.PostAsync("/api/places", form);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("location", (await Json(response)).GetProperty("message").GetString(), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CreatePlace_IgnoresServerOwnedFields()
        {
            var client = await _factory.SignedInClientAsync();
            var form = new MultipartFormDataContent
            {
                { new StringContent("Mass assignment test"), "Name" },
                { new StringContent("Unisex"), "Type" },
                { new StringContent("18.47"), "Lat" },
                { new StringContent("-69.93"), "Long" },
                { new StringContent("false"), "IsAproved" },
                { new StringContent("2030-01-01"), "LastVerifiedAt" },
            };

            var created = await Json(await client.PostAsync("/api/places", form));

            Assert.True(created.GetProperty("isAproved").GetBoolean());
            Assert.Equal(JsonValueKind.Null, created.GetProperty("lastVerifiedAt").ValueKind);
            Assert.True(created.GetProperty("isOwner").GetBoolean());
        }

        [Fact]
        public async Task OtherUsers_CannotEditSomeoneElsesPlace()
        {
            var places = await Json(await _factory.CreateClient().GetAsync("/api/places"));
            var notMine = places.EnumerateArray().First(p => p.GetProperty("ownerUsername").GetString() != "kevin");
            var kevin = await _factory.SignedInClientAsync("kevin@test.com");

            var response = await kevin.DeleteAsync($"/api/places/{notMine.GetProperty("id").GetString()}");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task ChangingPassword_EndsOldSessions_AndNewTokenWorks()
        {
            var client = _factory.CreateClient();
            var email = $"pw{Guid.NewGuid():N}"[..12] + "@test.com";
            var register = await client.PostAsJsonAsync("/api/account/register", new
            {
                email,
                username = "pw" + Guid.NewGuid().ToString("N")[..10],
                displayName = "Password tester",
                password = "FirstPass1"
            });
            Assert.Equal(HttpStatusCode.OK, register.StatusCode);
            var oldToken = (await Json(register)).GetProperty("token").GetString();

            // Warm the session cache with the old token first.
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", oldToken);
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/account")).StatusCode);

            var change = await client.PostAsJsonAsync("/api/account/password", new { currentPassword = "FirstPass1", newPassword = "SecondPass2" });
            Assert.Equal(HttpStatusCode.OK, change.StatusCode);
            var newToken = (await Json(change)).GetProperty("token").GetString();

            Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/account")).StatusCode);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/account")).StatusCode);
        }

        [Fact]
        public async Task DeletingAccount_EndsTheSession()
        {
            var client = _factory.CreateClient();
            var register = await client.PostAsJsonAsync("/api/account/register", new
            {
                email = $"del{Guid.NewGuid():N}"[..12] + "@test.com",
                username = "del" + Guid.NewGuid().ToString("N")[..10],
                displayName = "Leaving",
                password = "LeavingNow1"
            });
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", (await Json(register)).GetProperty("token").GetString());
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/account")).StatusCode);

            Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync("/api/account")).StatusCode);
            Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/account")).StatusCode);
        }

        [Fact]
        public async Task Register_RejectsBadUsernames()
        {
            var response = await _factory.CreateClient().PostAsJsonAsync("/api/account/register", new
            {
                email = "weird@test.com",
                username = "../admin",
                displayName = "Weird",
                password = "GoodPass1"
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
