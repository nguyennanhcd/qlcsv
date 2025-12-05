using QLCSV.Models;
using Microsoft.EntityFrameworkCore;

namespace QLCSV.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context, IConfiguration configuration)
        {
            // Check if admin already exists
            if (await context.Users.AnyAsync(u => u.Role == "admin"))
            {
                Console.WriteLine("Admin user already exists. Skipping seed.");
                return;
            }

            Console.WriteLine("Creating default admin user...");

            // Get admin credentials from environment variables or use defaults
            var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL") ?? "admin@qlcsv.com";
            var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "Admin@123456";
            var adminName = Environment.GetEnvironmentVariable("ADMIN_NAME") ?? "System Administrator";

            // Create admin user
            var adminUser = new User
            {
                Email = adminEmail,
                FullName = adminName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                Role = "admin",
                IsActive = true,
                EmailVerified = true, // Admin doesn't need email verification
                EmailVerificationToken = null,
                EmailVerificationTokenExpiry = null,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            context.Users.Add(adminUser);
            await context.SaveChangesAsync();

            Console.WriteLine($"✅ Admin user created successfully!");
            Console.WriteLine($"   Email: {adminEmail}");
            Console.WriteLine($"   Password: {adminPassword}");
            Console.WriteLine($"   ⚠️  IMPORTANT: Change the password after first login!");
        }
    }
}
