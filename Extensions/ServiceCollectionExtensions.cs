using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QLCSV.Data;
using QLCSV.Service;
using System.Security.Claims;
using System.Text;

namespace QLCSV.Extensions
{

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
            {
                var conn = configuration.GetConnectionString("DefaultConnection");
                var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

                // Parse Railway/PostgreSQL DATABASE_URL format: postgres://user:password@host:port/database
                if (!string.IsNullOrWhiteSpace(databaseUrl))
                {
                    try
                    {
                        // Replace postgres:// with postgresql:// for proper URI parsing
                        var uriString = databaseUrl.Replace("postgres://", "postgresql://");
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
                        conn = databaseUrl;
                    }
                }

                options.UseNpgsql(conn ?? throw new InvalidOperationException("Connection string not found"));
            });

            return services;
        }

        /// <summary>
        /// Adds JWT-based authentication and authorization services
        /// </summary>
        public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    // Try environment variable first, fallback to appsettings
                    var key = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
                              ?? configuration["Jwt:Key"];

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
                        ValidIssuer = configuration["Jwt:Issuer"],
                        ValidAudience = configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                        ClockSkew = TimeSpan.Zero,
                        RoleClaimType = ClaimTypes.Role,
                        NameClaimType = ClaimTypes.NameIdentifier
                    };
                });

            services.AddAuthorization();

            return services;
        }

        /// <summary>
        /// Adds application-specific services (JWT, Email, etc.)
        /// </summary>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // HttpClient for EmailService
            services.AddHttpClient<IEmailService, EmailService>();

            // JWT Service
            services.AddScoped<IJwtService, JwtService>();

            // Add request size limits (10MB default)
            services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10 MB
            });

            return services;
        }

        /// <summary>
        /// Adds Swagger/OpenAPI with JWT authentication support
        /// </summary>
        public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
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

            return services;
        }

        /// <summary>
        /// Configures Kestrel server options (request size limits)
        /// </summary>
        public static IWebHostBuilder ConfigureWebServer(this IWebHostBuilder webHost)
        {
            webHost.ConfigureKestrel(options =>
            {
                options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10 MB
            });

            return webHost;
        }
    }
}
