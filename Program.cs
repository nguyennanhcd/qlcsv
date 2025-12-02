using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QLCSV.Data;
using QLCSV.Middleware;
using QLCSV.Service;
using System.Security.Claims;
using System.Text;
using DotNetEnv;

// Load .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger with JWT support
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "QLCSV API",
        Version = "v1",
        Description = "API for Alumni Management System"
    });

    // Add JWT authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token in the format: {your token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var conn = builder.Configuration.GetConnectionString("DefaultConnection");
    var renderConn = Environment.GetEnvironmentVariable("DATABASE_URL");

    // Parse Render.com DATABASE_URL format: postgres://user:password@host:port/database
    if (!string.IsNullOrWhiteSpace(renderConn))
    {
        try
        {
            // Replace postgres:// with postgresql:// for proper URI parsing
            var uriString = renderConn.Replace("postgres://", "postgresql://");
            var uri = new Uri(uriString);
            var db = uri.AbsolutePath.Trim('/');
            var userInfo = uri.UserInfo.Split(':');
            var port = uri.Port > 0 ? uri.Port : 5432; // Default to 5432 if port not specified
            conn = $"Host={uri.Host};Port={port};Database={db};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
        }
        catch (Exception ex)
        {
            // Log the error for debugging
            Console.WriteLine($"Failed to parse DATABASE_URL: {ex.Message}");
            // If parsing fails, try using as-is
            conn = renderConn;
        }
    }

    options.UseNpgsql(conn ?? throw new InvalidOperationException("Connection string not found"));
});

// JWT Service
builder.Services.AddScoped<IJwtService, JwtService>();

// Authentication
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Try environment variable first, fallback to appsettings
        var key = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") 
                  ?? builder.Configuration["Jwt:Key"];
        
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException("JWT_SECRET_KEY is not configured");
        }

        if (key.Length < 32)
        {
            throw new InvalidOperationException("JWT_SECRET_KEY must be at least 32 characters");
        }

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ClockSkew = TimeSpan.Zero, // không cộng thêm thời gian trễ
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.NameIdentifier
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Global exception handling (MUST be first)
app.UseGlobalExceptionHandler();

// Rate limiting for authentication endpoints
app.UseRateLimiting();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Production: Add security headers
    app.Use(async (context, next) =>
    {
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
        context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
        await next();
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
