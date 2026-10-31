using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace College_Web_Application.Models
{
    public class TeacherCourseAssignment
    {
        public int Id { get; set; }

        [Required]
        public int StaffId { get; set; }

        [ForeignKey(nameof(StaffId))]
        public virtual StaffProfile? Staff { get; set; }

        [Required]
        public int CourseId { get; set; }

        [ForeignKey(nameof(CourseId))]
        public virtual Course? Course { get; set; }

        [Required]
        [StringLength(50)]
        public string AcademicSession { get; set; } = "Fall 2026"; // e.g., "Fall 2026" or "2026-2027"

        [Required]
        [StringLength(50)]
        public string Section { get; set; } = "A"; // e.g., "A", "Section A", "Morning"

        public bool IsActive { get; set; } = true;

        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
    }
}
