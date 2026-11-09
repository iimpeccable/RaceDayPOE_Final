using Microsoft.EntityFrameworkCore;
using RacingDays.API.Models;

namespace RacingDays.API.Data
{
    public class RacingDaysDbContext : DbContext
    {
        public RacingDaysDbContext(DbContextOptions<RacingDaysDbContext> options) : base(options)
        {
        }

        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Registration> Registrations { get; set; }
        public DbSet<Result> Results { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure cascading delete rules to prevent SQL Server multiple cascade path cycles
            modelBuilder.Entity<Registration>()
                .HasOne(r => r.Event)
                .WithMany(e => e.Registrations)
                .HasForeignKey(r => r.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Registration>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Result relationship to Registration
            modelBuilder.Entity<Result>()
                .HasOne(r => r.Registration)
                .WithMany(reg => reg.Results)
                .HasForeignKey(r => r.RegistrationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed initial Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, Name = "Organiser" },
                new Role { RoleId = 2, Name = "Participant" }
            );

            // Seed default Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = 1, Name = "Open 10km Run", Type = "Distance" },
                new Category { CategoryId = 2, Name = "Elite Marathon", Type = "Distance" },
                new Category { CategoryId = 3, Name = "Junior Cycling", Type = "Age/Distance" }
            );
        }
    }
}