using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Tests.Integration
{
    /// <summary>Runs the real API (Development settings, demo data) against a throwaway SQLite file.</summary>
    public class ApiFactory : WebApplicationFactory<Program>
    {
        private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"peepoo-it-{Guid.NewGuid():N}.db");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.UseSetting("Database:Provider", "Sqlite");
            builder.UseSetting("ConnectionStrings:DefaultConnection", $"Data Source={_dbPath}");
            builder.UseSetting("RateLimits:AuthPerMinute", "1000");
            builder.UseSetting("RateLimits:WritesPerMinute", "1000");
        }

        public async Task<HttpClient> SignedInClientAsync(string email = "starling@test.com", string password = Persistence.Seed.DemoPassword)
        {
            var client = CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await TokenAsync(client, email, password));
            return client;
        }

        public static async Task<string> TokenAsync(HttpClient client, string email, string password)
        {
            var response = await client.PostAsJsonAsync("/api/account/login", new { email, password });
            response.EnsureSuccessStatusCode();
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return json.RootElement.GetProperty("token").GetString();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            try { File.Delete(_dbPath); } catch (IOException) { }
        }
    }
}
