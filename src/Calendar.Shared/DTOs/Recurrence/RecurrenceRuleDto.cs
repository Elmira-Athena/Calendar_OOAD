using Calendar.Core.Enums;

namespace Calendar.Shared.DTOs.Recurrence;

public class RecurrenceRuleDto
{
    public Guid Id { get; set; }
    public RecurrenceType Type { get; set; }
    public int Interval { get; set; }
    public DayOfWeek? DayOfWeek { get; set; }
    public int? DayOfMonth { get; set; }
    public int? MonthOfYear { get; set; }
    public DateTime? RecurrenceEndDate { get; set; }
    public int? MaxOccurrences { get; set; }
}
