using Microsoft.EntityFrameworkCore;
using Serilog;
using TourManagement.Application.Extensions;
using TourManagement.Infrastructure.Data;
using TourManagement.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddRazorPages();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(int.Parse(Environment.GetEnvironmentVariable("SESSION_TIMEOUT_MINUTES") ?? "30"));
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<TourManagementDbContext>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

if (!bool.Parse(Environment.GetEnvironmentVariable("DISABLE_HTTPS_REDIRECT") ?? "false"))
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapRazorPages();

try
{
    Log.Information("Starting Tour Management application");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}
