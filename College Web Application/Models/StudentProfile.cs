using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace College_Web_Application.Models
{
    public enum AcademicStatus
    {
        Active = 0,
        Rusticated_ParentRequired = 1,
        Debarred_ShortAttendance = 2,
        StruckOff = 3,
        Graduated = 4
    }

    public class StudentProfile
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string RollNumber { get; set; } = string.Empty; // Unique Index

        [Required]
        [StringLength(50)]
        public string RegistrationNo { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string FatherName { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string Gender { get; set; } = "Male"; // Male, Female

        [Required]
        [StringLength(20)]
        public string CNIC { get; set; } = string.Empty; // Unique Index (e.g. 15602-1234567-1)

        public DateTime DateOfBirth { get; set; }

        [Required]
        [StringLength(25)]
        public string Phone { get; set; } = string.Empty;

        [StringLength(25)]
        public string? GuardianPhone { get; set; }

        [NotMapped]
        public string GuardianContact => !string.IsNullOrEmpty(GuardianPhone) ? GuardianPhone : Phone;

        [Required]
        [StringLength(100)]
        public string DomicileDistrict { get; set; } = "Swat";

        [Required]
        [StringLength(255)]
        public string Address { get; set; } = string.Empty;

        [Required]
        public int ProgramId { get; set; }

        [ForeignKey(nameof(ProgramId))]
        public virtual AcademicProgram? Program { get; set; }

        public int CurrentSemester { get; set; } = 1;

        [Required]
        [StringLength(10)]
        public string Section { get; set; } = "A";

        [Column(TypeName = "decimal(4,2)")]
        public decimal CGPA { get; set; } = 0.00m;

        public DateTime AdmissionDate { get; set; } = DateTime.UtcNow;

        [StringLength(255)]
        public string? PhotoUrl { get; set; }

        [StringLength(100)]
        public string? IdentityUserId { get; set; }

        public bool IsActive { get; set; } = true;

        // Disciplinary & 3-Day Consecutive Absence Engine
        public AcademicStatus AcademicStatus { get; set; } = AcademicStatus.Active;

        public int ConsecutiveAbsentDays { get; set; } = 0;

        public virtual ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
        public virtual ICollection<ExamResult> ExamResults { get; set; } = new List<ExamResult>();
        public virtual ICollection<FeeChallan> FeeChallans { get; set; } = new List<FeeChallan>();
        public virtual ICollection<RusticationRecord> RusticationRecords { get; set; } = new List<RusticationRecord>();
        public virtual ICollection<ParentNotificationLog> NotificationLogs { get; set; } = new List<ParentNotificationLog>();
    }
}
