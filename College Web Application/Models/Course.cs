using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace College_Web_Application.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required]
        [StringLength(30)]
        public string CourseCode { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        public int CreditHours { get; set; } = 3;

        [Required]
        public int ProgramId { get; set; }

        [ForeignKey(nameof(ProgramId))]
        public virtual AcademicProgram? Program { get; set; }

        public int Semester { get; set; } = 1;

        public virtual ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
        public virtual ICollection<ExamResult> ExamResults { get; set; } = new List<ExamResult>();
    }
}
