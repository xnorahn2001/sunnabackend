using Microsoft.EntityFrameworkCore;
using SonnaBackend.Models;

namespace SonnaBackend.Data
{
    public class SonnaDbContext : DbContext
    {
        public SonnaDbContext(DbContextOptions<SonnaDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Project> Projects { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }
        
        // Content Tables
        public DbSet<News> News { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Camp> Camps { get; set; }
        public DbSet<Podcast> Podcasts { get; set; }
        public DbSet<Expert> Experts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Map models to specific table names if needed, or let EF handle it (PascalCase tables usually).
            // Default is Pluralized property names.

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}
