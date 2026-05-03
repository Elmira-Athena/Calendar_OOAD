using Calendar.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendar.Infrastructure.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.HasKey(x => x.Id);

        // TPH: EF Core will add a Discriminator column automatically.
        // "Appointment" for base, "GroupMeeting" for subclass.
        builder.HasDiscriminator<string>("Discriminator")
               .HasValue<Appointment>("Appointment")
               .HasValue<GroupMeeting>("GroupMeeting");

        // Optimize for date range query and conflict checks
        builder.HasIndex(x => new { x.UserId, x.StartTime, x.EndTime });

        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Location).HasMaxLength(500);
        builder.Property(x => x.Color).HasMaxLength(20);

        builder.HasOne(x => x.User)
               .WithMany(x => x.Appointments)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.RecurrenceRule)
               .WithMany(x => x.Appointments)
               .HasForeignKey(x => x.RecurrenceRuleId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.RecurrenceParent)
               .WithMany(x => x.RecurrenceInstances)
               .HasForeignKey(x => x.RecurrenceParentId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public class GroupMeetingConfiguration : IEntityTypeConfiguration<GroupMeeting>
{
    public void Configure(EntityTypeBuilder<GroupMeeting> builder)
    {
        // Index for fast group meeting lookup by name+time (used for auto-detect)
        builder.HasIndex(x => new { x.Name, x.StartTime, x.EndTime });

        builder.HasOne(x => x.CreatedByUser)
               .WithMany(x => x.CreatedMeetings)
               .HasForeignKey(x => x.CreatedByUserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
