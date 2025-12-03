using Microsoft.EntityFrameworkCore;
using QLCSV.Models;

namespace QLCSV.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSet
        public DbSet<User> Users => Set<User>();
        public DbSet<AlumniProfile> AlumniProfiles => Set<AlumniProfile>();
        public DbSet<Faculty> Faculties => Set<Faculty>();
        public DbSet<Major> Majors => Set<Major>();
        public DbSet<Batch> Batches => Set<Batch>();
        public DbSet<AlumniBatch> AlumniBatches => Set<AlumniBatch>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<EventRegistration> EventRegistrations => Set<EventRegistration>();

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Automatically update UpdatedAt for modified entities
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.Entity is User user)
                {
                    user.UpdatedAt = DateTimeOffset.UtcNow;
                }
                else if (entry.Entity is AlumniProfile profile)
                {
                    profile.UpdatedAt = DateTimeOffset.UtcNow;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==================== 1. User ====================
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Id).ValueGeneratedOnAdd();

                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Email).HasMaxLength(255).IsRequired();

                entity.Property(u => u.PasswordHash).HasMaxLength(255).IsRequired();
                entity.Property(u => u.FullName).HasMaxLength(255).IsRequired();

                entity.Property(u => u.Role)
                      .HasMaxLength(20)
                      .HasConversion<string>()
                      .HasDefaultValue("pending");

                entity.Property(u => u.IsActive).HasDefaultValue(true);

                entity.Property(u => u.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(u => u.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // ==================== 2. AlumniProfile ====================
            modelBuilder.Entity<AlumniProfile>(entity =>
            {
                entity.ToTable("alumni_profiles");
                entity.HasKey(a => a.Id);

                entity.HasIndex(a => a.StudentId).IsUnique();
                entity.Property(a => a.StudentId).HasMaxLength(20);

                entity.Property(a => a.IsPublic).HasDefaultValue(false);
                entity.Property(a => a.Country).HasMaxLength(100).HasDefaultValue("Việt Nam");

                // One-to-One với User
                entity.HasOne(a => a.User)
                      .WithOne(u => u.AlumniProfile)
                      .HasForeignKey<AlumniProfile>(a => a.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Với Faculty & Major (Restrict để không xóa nhầm)
                entity.HasOne(a => a.Faculty)
                      .WithMany(f => f.AlumniProfiles)
                      .HasForeignKey(a => a.FacultyId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Major)
                      .WithMany(m => m.AlumniProfiles)
                      .HasForeignKey(a => a.MajorId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ==================== 3. Faculty ====================
            modelBuilder.Entity<Faculty>(entity =>
            {
                entity.ToTable("faculties");
                entity.HasIndex(f => f.Name).IsUnique();
                entity.Property(f => f.Name).HasMaxLength(255).IsRequired();
                entity.Property(f => f.ShortName).HasMaxLength(50);
            });

            // ==================== 4. Major ====================
            modelBuilder.Entity<Major>(entity =>
            {
                entity.ToTable("majors");
                entity.HasOne(m => m.Faculty)
                      .WithMany(f => f.Majors)
                      .HasForeignKey(m => m.FacultyId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(m => new { m.FacultyId, m.Code })
                      .IsUnique()
                      .HasFilter("\"Code\" IS NOT NULL"); // cho phép Code = null
            });

            // ==================== 5. Batch ====================
            modelBuilder.Entity<Batch>(entity =>
            {
                entity.ToTable("batches");
                entity.HasIndex(b => new { b.GraduationYear, b.Name }).IsUnique();
                entity.Property(b => b.Name).HasMaxLength(100).IsRequired();
            });

            // ==================== 6. AlumniBatch (Many-to-Many) ====================
            modelBuilder.Entity<AlumniBatch>(entity =>
            {
                entity.ToTable("alumni_batches");
                entity.HasKey(ab => new { ab.AlumniId, ab.BatchId });

                entity.HasOne(ab => ab.Alumni)
                      .WithMany(a => a.AlumniBatches)
                      .HasForeignKey(ab => ab.AlumniId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ab => ab.Batch)
                      .WithMany(b => b.AlumniBatches)
                      .HasForeignKey(ab => ab.BatchId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ==================== 7. Event ====================
            modelBuilder.Entity<Event>(entity =>
            {
                entity.ToTable("events");
                entity.Property(e => e.Title).HasMaxLength(255).IsRequired();

                entity.HasOne(e => e.CreatedByUser)
                      .WithMany(u => u.EventsCreated)
                      .HasForeignKey(e => e.CreatedBy)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ==================== 8. EventRegistration ====================
            modelBuilder.Entity<EventRegistration>(entity =>
            {
                entity.ToTable("event_registrations");

                entity.HasOne(r => r.Event)
                      .WithMany(e => e.Registrations)
                      .HasForeignKey(r => r.EventId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.User)
                      .WithMany(u => u.EventRegistrations)
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(r => new { r.EventId, r.UserId }).IsUnique();

                entity.Property(r => r.Status)
                      .HasMaxLength(20)
                      .HasConversion<string>()
                      .HasDefaultValue("registered");
            });
        }
    }
}