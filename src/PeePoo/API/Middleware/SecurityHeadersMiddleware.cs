using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace API.Middleware
{
    /// <summary>Adds browser hardening headers to every response.</summary>
    public class SecurityHeadersMiddleware
    {
        // Everything the site needs is served from this origin; map tiles and uploaded photos may be remote images.
        private const string ContentSecurityPolicy =
            "default-src 'self'; img-src 'self' data: blob: https:; script-src 'self'; style-src 'self'; " +
            "font-src 'self'; connect-src 'self'; object-src 'none'; frame-ancestors 'none'; base-uri 'self'; form-action 'self'";

        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task InvokeAsync(HttpContext context)
        {
            context.Response.OnStarting(() =>
            {
                var headers = context.Response.Headers;
                headers["X-Content-Type-Options"] = "nosniff";
                headers["X-Frame-Options"] = "DENY";
                headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
                headers["Permissions-Policy"] = "camera=(), microphone=(), payment=(), geolocation=(self)";
                headers["Cross-Origin-Opener-Policy"] = "same-origin";

                // Swagger UI (development only) relies on inline scripts.
                if (!context.Request.Path.StartsWithSegments("/swagger"))
                    headers["Content-Security-Policy"] = ContentSecurityPolicy;
                return Task.CompletedTask;
            });
            return _next(context);
        }
    }
}
