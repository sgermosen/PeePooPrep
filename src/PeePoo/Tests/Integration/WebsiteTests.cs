using Microsoft.Extensions.DependencyInjection;
using Persistence;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Integration
{
    public class WebsiteTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public WebsiteTests(ApiFactory factory)
        {
            _factory = factory;
        }

        [Theory]
        [InlineData("/", "¿Dónde hay un")]
        [InlineData("/explorar", "Mapa de baños")]
        [InlineData("/negocios", "Para negocios")]
        [InlineData("/guias", "Guías")]
        [InlineData("/guias/banos-accesibles-que-revisar", "Baños accesibles")]
        [InlineData("/ayuda", "Preguntas frecuentes")]
        [InlineData("/privacidad", "Política de privacidad")]
        [InlineData("/terminos", "Términos de uso")]
        [InlineData("/eliminar-cuenta", "Eliminar tu cuenta")]
        [InlineData("/privacy", "Privacy Policy")]
        [InlineData("/terms", "Terms of Use")]
        [InlineData("/account-deletion", "Account &amp; data deletion")]
        [InlineData("/admin", "Moderación")]
        public async Task PublicPages_Render(string path, string expected)
        {
            var response = await _factory.CreateClient().GetAsync(path);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(expected, await response.Content.ReadAsStringAsync());
            Assert.Contains("script-src 'self'", response.Headers.GetValues("Content-Security-Policy").Single());
        }

        [Fact]
        public async Task PlacePage_ShowsThePlace_AndStructuredData()
        {
            var client = _factory.CreateClient();
            var place = await FirstPlaceAsync();

            var html = await client.GetStringAsync($"/lugar/{place.Id}");

            Assert.Contains(place.Name, html);
            Assert.Contains("\"@type\":\"PublicToilet\"", html);
            Assert.Contains("og:title", html);
        }

        [Fact]
        public async Task UnknownPages_ReturnTheFriendly404()
        {
            var client = _factory.CreateClient();

            var page = await client.GetAsync("/esto-no-existe");
            Assert.Equal(HttpStatusCode.NotFound, page.StatusCode);
            Assert.Contains("no lleva a ningún baño", await page.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/lugar/{Guid.NewGuid()}")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/guias/no-existe")).StatusCode);
        }

        [Fact]
        public async Task HiddenPlaces_AreNotPublic()
        {
            Guid hiddenId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DataContext>();
                var place = db.Places.OrderBy(p => p.CreatedAt).First(p => p.IsAproved);
                place.IsAproved = false;
                db.SaveChanges();
                hiddenId = place.Id;
            }

            try
            {
                var client = _factory.CreateClient();
                Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/lugar/{hiddenId}")).StatusCode);
                Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/places/{hiddenId}")).StatusCode);
                Assert.DoesNotContain(hiddenId.ToString(), await client.GetStringAsync("/sitemap.xml"));
            }
            finally
            {
                using var scope = _factory.Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<DataContext>();
                db.Places.Single(p => p.Id == hiddenId).IsAproved = true;
                db.SaveChanges();
            }
        }

        [Fact]
        public async Task SitemapAndRobots_ListPublicContent()
        {
            var client = _factory.CreateClient();
            var place = await FirstPlaceAsync();

            var sitemap = await client.GetStringAsync("/sitemap.xml");
            Assert.Contains($"/lugar/{place.Id}", sitemap);
            Assert.Contains("/guias/salir-con-bebes", sitemap);

            var robots = await client.GetStringAsync("/robots.txt");
            Assert.Contains("Disallow: /admin", robots);
            Assert.Contains("/sitemap.xml", robots);
        }

        private async Task<Domain.Place> FirstPlaceAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DataContext>();
            // Ensure the app has started and seeded.
            await _factory.CreateClient().GetAsync("/health");
            return db.Places.Where(p => p.IsAproved).OrderByDescending(p => p.CreatedAt).First();
        }
    }
}
