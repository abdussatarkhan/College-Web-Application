using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace College_Web_Application.Models
{
    public class ParentNotificationLog
    {
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [ForeignKey(nameof(StudentId))]
        public virtual StudentProfile? Student { get; set; }

        [Required]
        [StringLength(50)]
        public string AlertType { get; set; } = "FSc_3DayRustication"; // FSc_3DayRustication, BS_AttendanceDebarment, FeeChallan, Admission

        [Required]
        [StringLength(25)]
        public string ParentPhone { get; set; } = string.Empty;

        [Required]
        public string BilingualMessage { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public bool IsDelivered { get; set; } = true;

        [StringLength(100)]
        public string? DispatchedByUserId { get; set; }
    }
}
