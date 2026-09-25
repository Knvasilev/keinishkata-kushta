using KeinishkataKushta.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace KeinishkataKushta.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Room> Rooms => Set<Room>();
        public DbSet<GalleryImage> GalleryImages => Set<GalleryImage>();
        public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
        public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
        public DbSet<AvailabilityBlock> AvailabilityBlocks => Set<AvailabilityBlock>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AvailabilityBlock>(entity =>
            {
                entity.ToTable("AvailabilityBlocks", table =>
                {
                    table.HasCheckConstraint("CK_AvailabilityBlocks_Dates", "[Departure] > [Arrival]");
                    table.HasCheckConstraint("CK_AvailabilityBlocks_Kind", "[Kind] IN (0, 1)");
                });
                entity.Property(e => e.Notes).HasMaxLength(1000);
                entity.Property(e => e.RoomsUsed).HasMaxLength(500);
                entity.Property(e => e.Version).IsRowVersion();
                entity.HasIndex(e => new { e.Arrival, e.Departure });
            });

            modelBuilder.Entity<AdminUser>(entity =>
            {
                entity.Property(e => e.Username).HasMaxLength(100).IsRequired();
                entity.Property(e => e.NormalizedUsername).HasMaxLength(100).IsRequired();
                entity.Property(e => e.PasswordHash).HasMaxLength(1000).IsRequired();
                entity.HasIndex(e => e.NormalizedUsername).IsUnique();
            });

            modelBuilder.Entity<Room>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Slug).HasMaxLength(200).IsRequired();
                entity.HasIndex(e=> e.Slug).IsUnique();
                entity.Property(e => e.PricePerNight).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Description).HasMaxLength(4000);
            });

            modelBuilder.Entity<GalleryImage>(entity =>
            {
                entity.Property(e => e.FileName).HasMaxLength(260).IsRequired();
                entity.Property(e => e.Caption).HasMaxLength(500);
                entity.HasIndex(e => e.RoomId);
                entity.HasIndex(e => new { e.RoomId, e.IsCover })
                    .HasFilter("[IsCover] = 1 AND [RoomId] IS NOT NULL")
                    .IsUnique();
            });

            modelBuilder.Entity<ContactMessage>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Phone).HasMaxLength(50);
                entity.Property(e => e.Message).HasMaxLength(4000).IsRequired();
            });
        }
    }
}
