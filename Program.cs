using DotNetEnv;
using QLCSV.Extensions;
using QLCSV.Middleware;

// Load .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add custom services via extension methods
builder.Services.AddDatabaseServices(builder.Configuration);
builder.Services.AddAuthenticationServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddSwaggerServices();

// Configure web server
builder.WebHost.ConfigureWebServer();

var app = builder.Build();

// Apply database migrations and seed data
await app.UseDatabaseMigrationAsync();

// Configure middleware pipeline
app.UseGlobalExceptionHandler();
app.UseRateLimiting();
app.UseSwaggerConfiguration(app.Environment);

if (!app.Environment.IsDevelopment())
{
    app.UseSecurityHeaders();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
