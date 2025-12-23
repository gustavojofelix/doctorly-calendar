using Calendar.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Calendar.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public DbSet<CalendarEvent> Events => Set<CalendarEvent>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CalendarEvent>(b =>
        {
            b.ToTable("Events");
            b.HasKey(x => x.Id);

            b.Property(x => x.Title).IsRequired().HasMaxLength(200);
            b.Property(x => x.Description).HasMaxLength(2000);
            b.Property(x => x.StartTime).IsRequired();
            b.Property(x => x.EndTime).IsRequired();
            b.Property(x => x.Status).IsRequired();

            b.Property(x => x.RowVersion) //.IsRowVersion()
                .IsConcurrencyToken();

            b.OwnsMany(
                x => x.Attendees,
                ab =>
                {
                    ab.ToTable("EventAttendees");
                    ab.WithOwner().HasForeignKey("EventId");
                    ab.HasKey(x => x.Id);
                    ab.Property(x => x.Name).IsRequired().HasMaxLength(200);
                    ab.Property(x => x.Email).IsRequired().HasMaxLength(320);
                    ab.Property(x => x.Status).IsRequired();

                    ab.HasIndex("EventId", nameof(Attendee.Email)).IsUnique();
                }
            );
        });
    }
}
