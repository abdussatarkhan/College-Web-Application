using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace College_Web_Application.Models
{
    public class AttendanceCondonation
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
        public decimal ActualAttendancePercentage { get; set; } // e.g. 68.50%

        [Required]
        [StringLength(1000)]
        public string ReasonAndMedicalProof { get; set; } = string.Empty;

        [StringLength(50)]
        public string? FineChallanNumber { get; set; } // e.g. BOK-COND-2026-001

        public bool IsApprovedByPrincipal { get; set; } = false;

        [StringLength(100)]
        public string? ApprovedByUserId { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
