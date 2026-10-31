using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace College_Web_Application.Models
{
    public class AttendanceRecord
    {
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [ForeignKey(nameof(StudentId))]
        public virtual StudentProfile? Student { get; set; }

        [Required]
        public int CourseId { get; set; }

        [ForeignKey(nameof(CourseId))]
        public virtual Course? Course { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow.Date;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Present"; // Present, Absent, Leave

        public int? MarkedByStaffId { get; set; }

        [ForeignKey(nameof(MarkedByStaffId))]
        public virtual StaffProfile? MarkedByStaff { get; set; }
    }
}
