using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace College_Web_Application.Models
{
    public class StaffProfile
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string EmployeeCode { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(80)]
        public string Designation { get; set; } = "Lecturer"; // Professor, Associate Professor, Assistant Professor, Lecturer, Clerk

        [Required]
        [StringLength(20)]
        public string BPSGrade { get; set; } = "BPS-17"; // BPS-17, BPS-18, BPS-19, BPS-20, BPS-11, BPS-14

        [Required]
        [StringLength(200)]
        public string Qualification { get; set; } = string.Empty;

        public int? DepartmentId { get; set; }

        [ForeignKey(nameof(DepartmentId))]
        public virtual Department? Department { get; set; }

        public DateTime JoiningDate { get; set; } = DateTime.UtcNow;

        [StringLength(255)]
        public string? PhotoUrl { get; set; }

        [StringLength(100)]
        public string? IdentityUserId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
