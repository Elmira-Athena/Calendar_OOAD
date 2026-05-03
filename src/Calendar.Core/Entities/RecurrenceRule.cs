using Calendar.Core.Enums;

namespace Calendar.Core.Entities;

public class RecurrenceRule
{
    public Guid Id { get; set; }
    public RecurrenceType Type { get; set; }
    public int Interval { get; set; }
    public DayOfWeek? DayOfWeek { get; set; }
    public int? DayOfMonth { get; set; }
    public int? MonthOfYear { get; set; }
    public DateTime? RecurrenceEndDate { get; set; }
    public int? MaxOccurrences { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
