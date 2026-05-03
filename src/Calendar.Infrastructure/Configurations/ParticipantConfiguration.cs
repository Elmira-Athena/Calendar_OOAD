using Calendar.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendar.Infrastructure.Configurations;

public class ParticipantConfiguration : IEntityTypeConfiguration<Participant>
{
    public void Configure(EntityTypeBuilder<Participant> builder)
    {
        builder.HasKey(x => x.Id);

        // A user can only participate in a given meeting once
        builder.HasIndex(x => new { x.MeetingId, x.UserId }).IsUnique();

        builder.HasOne(x => x.Meeting)
               .WithMany(x => x.Participants)
               .HasForeignKey(x => x.MeetingId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
               .WithMany(x => x.Participations)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
