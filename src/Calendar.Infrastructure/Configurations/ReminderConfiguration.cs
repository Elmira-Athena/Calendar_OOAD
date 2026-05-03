using Calendar.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendar.Infrastructure.Configurations;

public class ReminderConfiguration : IEntityTypeConfiguration<Reminder>
{
    public void Configure(EntityTypeBuilder<Reminder> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.HasOne(x => x.Appointment)
               .WithMany(x => x.Reminders)
               .HasForeignKey(x => x.AppointmentId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
