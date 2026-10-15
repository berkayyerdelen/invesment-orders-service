using Microsoft.EntityFrameworkCore;
using SlotBooking.Api.Models;

namespace SlotBooking.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Room>(entity =>
        {
            entity.ToTable("rooms");
            entity.HasKey(room => room.Id);
            entity.Property(room => room.Id).HasColumnName("id");
            entity.Property(room => room.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.ToTable("bookings");
            entity.HasKey(booking => booking.Id);

            entity.Property(booking => booking.Id).HasColumnName("id");
            entity.Property(booking => booking.RoomId).HasColumnName("room_id");
            entity.Property(booking => booking.StartsAt).HasColumnName("starts_at").HasColumnType("timestamptz");
            entity.Property(booking => booking.EndsAt).HasColumnName("ends_at").HasColumnType("timestamptz");
            entity.Property(booking => booking.GuestName).HasColumnName("guest_name").HasMaxLength(Booking.MaxGuestNameLength).IsRequired();
            entity.Property(booking => booking.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);

            // Optimistic concurrency: EF adds "WHERE xmin = @version" to every UPDATE and
            // reads the new xmin back after each write.
            entity.Property(booking => booking.Version).UseXminAsConcurrencyToken();

            entity.HasOne(booking => booking.Room)
                .WithMany()
                .HasForeignKey(booking => booking.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(booking => new { booking.RoomId, booking.StartsAt });
        });
    }
}
