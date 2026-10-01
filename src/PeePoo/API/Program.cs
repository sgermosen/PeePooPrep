using API.Extensions;
using API.Middleware;
using API.Services;
using Application.Places;
using Domain;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Persistence;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(opt =>
{
    var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    opt.Filters.Add(new AuthorizeFilter(policy));
})
    .AddNewtonsoftJson(options =>
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore)
    .ConfigureApiBehaviorOptions(options =>
    {
        // Clients show "message"; "errors" keeps the per-field detail.
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .ToDictionary(e => e.Key, e => e.Value!.Errors.Select(x => x.ErrorMessage).ToArray());
            var first = errors.Values.SelectMany(v => v).FirstOrDefault(m => !string.IsNullOrWhiteSpace(m))
                        ?? "Some of the information is not valid.";
            return new BadRequestObjectResult(new { message = first, errors });
        };
    });

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Create>();

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);

builder.Services.Configure<FormOptions>(o => o.MultipartBodyLengthLimit = Application.Core.PhotoRules.MaxBytes + 1024 * 1024);

var authPerMinute = builder.Configuration.GetValue("RateLimits:AuthPerMinute", 10);
var writesPerMinute = builder.Configuration.GetValue("RateLimits:WritesPerMinute", 60);
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (ctx, token) =>
    {
        ctx.HttpContext.Response.ContentType = "application/json";
        await ctx.HttpContext.Response.WriteAsync("{\"message\":\"Too many requests. Please wait a moment and try again.\"}", token);
    };

    // Sign-in / sign-up attempts per minute per IP address.
    options.AddPolicy("auth", ctx => RateLimitPartition.GetFixedWindowLimiter(
        ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { Window = TimeSpan.FromMinutes(1), PermitLimit = authPerMinute, QueueLimit = 0 }));

    // Writes per minute per signed-in user (or IP when anonymous).
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
    {
        if (HttpMethods.IsGet(ctx.Request.Method) || HttpMethods.IsHead(ctx.Request.Method) || HttpMethods.IsOptions(ctx.Request.Method))
            return RateLimitPartition.GetNoLimiter("read");
        var key = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter("write:" + key,
            _ => new FixedWindowRateLimiterOptions { Window = TimeSpan.FromMinutes(1), PermitLimit = writesPerMinute, QueueLimit = 0 });
    });
});

builder.Services.AddHealthChecks().AddDbContextCheck<DataContext>();

if (!builder.Environment.IsDevelopment())
    builder.Services.AddHsts(o => { o.MaxAge = TimeSpan.FromDays(365); o.IncludeSubDomains = true; });

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PeePoo API v1"));
}
else
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

// Photos stored by LocalPhotoAccessor (used when Cloudinary is not configured).
var uploadsPath = Path.Combine(app.Environment.WebRootPath ?? Path.Combine(app.Environment.ContentRootPath, "wwwroot"),
    LocalPhotoAccessor.UploadFolder);
Directory.CreateDirectory(uploadsPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/" + LocalPhotoAccessor.UploadFolder,
    ContentTypeProvider = new FileExtensionContentTypeProvider(
        new Dictionary<string, string>(LocalPhotoAccessor.ContentTypes, StringComparer.OrdinalIgnoreCase))
});

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

await InitializeDatabaseAsync(app);

await app.RunAsync();

static async System.Threading.Tasks.Task InitializeDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<DataContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        if (context.Database.IsSqlite())
            await DevDatabase.EnsureCurrentAsync(context, logger);
        else
            await context.Database.MigrateAsync();

        await Seed.EnsureRolesAndAdminAsync(userManager, roleManager,
            app.Configuration["Seed:AdminEmail"], app.Configuration["Seed:AdminPassword"]);

        if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Seed:DemoData"))
            await Seed.SeedDemoDataAsync(context, userManager);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while preparing the database");
    }
}

public partial class Program { }
