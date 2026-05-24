// DevSum 2026 Demo App - .NET 10 (supported) version
// Same code, same dependencies as the .NET 6 version - just patched
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// In-memory database + Identity - a realistic pattern for any .NET app with auth
builder.Services.AddDbContext<IdentityDbContext>(opt => opt.UseInMemoryDatabase("demo"));
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<IdentityDbContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new
{
    message = "Hello from DevSum 2026!",
    framework = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
    hostname = Environment.MachineName,
    timestamp = DateTime.UtcNow
}));

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

// ⚠️  DELIBERATELY VULNERABLE - path traversal via unsanitized user input
// A developer might write this to serve user-uploaded reports
app.MapGet("/download", (string file) =>
{
    var basePath = Path.Combine(app.Environment.ContentRootPath, "data");
    var filePath = Path.Combine(basePath, file);

    if (!File.Exists(filePath))
        return Results.NotFound(new { error = "File not found", file });

    var content = File.ReadAllText(filePath);
    return Results.Ok(new { file, content });
});

app.Run();
