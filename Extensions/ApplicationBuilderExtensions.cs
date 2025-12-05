using Microsoft.EntityFrameworkCore;
using QLCSV.Data;
using QLCSV.Middleware;

namespace QLCSV.Extensions
{
    /// <summary>
    /// Extension methods for configuring the HTTP request pipeline
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Applies database migrations and seeds initial data
        /// </summary>
        public static async Task<IApplicationBuilder> UseDatabaseMigrationAsync(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();

                try
                {
                    var context = services.GetRequiredService<AppDbContext>();
                    var configuration = services.GetRequiredService<IConfiguration>();

                    logger.LogInformation("Applying database migrations...");
                    await context.Database.MigrateAsync();
                    Console.WriteLine("✅ Database migrations applied successfully!");

                    // Seed initial data (admin user)
                    await DbSeeder.SeedAsync(context, configuration);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Database migration error");
                    Console.WriteLine($"⚠️  Database Error: {ex.Message}");
                    Console.WriteLine("⚠️  Application will continue. Database operations may fail if schema is incompatible.");
                }
            }

            return app;
        }

        /// <summary>
        /// Adds security headers for production environment
        /// </summary>
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        {
            return app.Use(async (context, next) =>
            {
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                context.Response.Headers["X-Frame-Options"] = "DENY";
                context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
                context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
                await next();
            });
        }

        /// <summary>
        /// Configures Swagger UI based on environment
        /// </summary>
        public static IApplicationBuilder UseSwaggerConfiguration(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            return app;
        }
    }
}
