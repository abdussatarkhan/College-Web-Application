using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace College_Web_Application.Models
{
    public enum ExamSubmissionStatus
    {
        Draft = 0,
        SubmittedToController = 1,
        ApprovedByController = 2,
        ReturnedForCorrection = 3
    }

    public class ExamResult
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

        public int Semester { get; set; } = 1;

        [Column(TypeName = "decimal(5,2)")]
        public decimal MidTermMarks { get; set; } = 0.00m; // Max 30

        [Column(TypeName = "decimal(5,2)")]
        public decimal SessionalMarks { get; set; } = 0.00m; // Max 20

        [Column(TypeName = "decimal(5,2)")]
        public decimal FinalMarks { get; set; } = 0.00m; // Max 50

        [Column(TypeName = "decimal(5,2)")]
        public decimal TotalMarks { get; set; } = 0.00m; // Max 100

        [StringLength(25)]
        public string Grade { get; set; } = "F"; // A, A-, B+, B, B-, C+, C, C-, D, F, SA (Short Attendance)

        [Column(TypeName = "decimal(4,2)")]
        public decimal GradePoints { get; set; } = 0.00m; // 0.00 to 4.00

        // Multi-Stage Approval Pipeline Properties
        public ExamSubmissionStatus SubmissionStatus { get; set; } = ExamSubmissionStatus.Draft;

        public int? SubmittedByStaffId { get; set; }

        [ForeignKey(nameof(SubmittedByStaffId))]
        public virtual StaffProfile? SubmittedByStaff { get; set; }

        public DateTime? SubmittedAt { get; set; }

        [StringLength(100)]
        public string? VerifiedByControllerId { get; set; }

        public DateTime? VerifiedAt { get; set; }

        [StringLength(500)]
        public string? ControllerRemarks { get; set; }

        // BS 4-Year 75% Attendance Compliance Properties
        [Column(TypeName = "decimal(5,2)")]
        public decimal AttendancePercentageAtExam { get; set; } = 100.00m;

        public bool IsDebarredForShortAttendance { get; set; } = false;
    }
}
