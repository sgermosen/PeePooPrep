using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Persistence;
using System;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Text;

namespace API.Site
{
    public static class SeoEndpoints
    {
        private static readonly string[] StaticPaths =
        {
            "/", "/explorar", "/negocios", "/guias", "/ayuda",
            "/privacidad", "/terminos", "/eliminar-cuenta", "/privacy", "/terms", "/account-deletion"
        };

        public static IEndpointRouteBuilder MapSeoEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/robots.txt", (HttpContext ctx, IOptions<SiteOptions> site) =>
                Results.Text($"User-agent: *\nDisallow: /admin\nDisallow: /api/\nSitemap: {Origin(ctx, site.Value)}/sitemap.xml\n", "text/plain"));

            app.MapGet("/sitemap.xml", async (HttpContext ctx, IOptions<SiteOptions> site, DataContext db) =>
            {
                var origin = Origin(ctx, site.Value);
                var places = await db.Places.Where(p => p.IsAproved)
                    .OrderByDescending(p => p.CreatedAt)
                    .Select(p => new { p.Id, Updated = p.LastVerifiedAt ?? p.CreatedAt })
                    .Take(45000)
                    .ToListAsync();

                var xml = new StringBuilder();
                xml.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">\n");
                void Url(string path, DateTime? updated = null)
                {
                    xml.Append("  <url><loc>").Append(SecurityElement.Escape(origin + path)).Append("</loc>");
                    if (updated.HasValue) xml.Append("<lastmod>").Append(updated.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)).Append("</lastmod>");
                    xml.Append("</url>\n");
                }
                foreach (var path in StaticPaths) Url(path);
                foreach (var guide in Guides.All) Url("/guias/" + guide.Slug, guide.Published);
                foreach (var place in places) Url("/lugar/" + place.Id, place.Updated);
                xml.Append("</urlset>\n");
                return Results.Text(xml.ToString(), "application/xml", Encoding.UTF8);
            });

            return app;
        }

        private static string Origin(HttpContext ctx, SiteOptions site) =>
            !string.IsNullOrWhiteSpace(site.BaseUrl) ? site.BaseUrl.TrimEnd('/') : $"{ctx.Request.Scheme}://{ctx.Request.Host}";
    }
}
