namespace Calendar.Core.Entities;

/// <summary>
/// GroupMeeting kế thừa từ Appointment (TPH).
/// Được hiển thị trên lịch của TẤT CẢ người dùng.
/// Chỉ người tạo (CreatedByUserId) mới có quyền sửa/xóa.
/// </summary>
public class GroupMeeting : Appointment
{
    /// <summary>
    /// Id của người tạo cuộc họp (đồng thời cũng là UserId từ Appointment cha)
    /// </summary>
    public Guid CreatedByUserId { get; set; }

    // Navigation
    public User CreatedByUser { get; set; } = null!;
    public ICollection<Participant> Participants { get; set; } = new List<Participant>();
}
