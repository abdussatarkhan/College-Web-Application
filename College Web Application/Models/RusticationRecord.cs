using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace College_Web_Application.Models
{
    public class RusticationRecord
    {
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [ForeignKey(nameof(StudentId))]
        public virtual StudentProfile? Student { get; set; }

        public DateTime RusticationDate { get; set; } = DateTime.UtcNow;

        [StringLength(250)]
        public string? AbsentDatesSummary { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;

        public int? CourseId { get; set; }

        [ForeignKey(nameof(CourseId))]
        public virtual Course? Course { get; set; }

        public bool IsReinstated { get; set; } = false;

        [StringLength(150)]
        public string? ParentNameVisited { get; set; }

        [StringLength(25)]
        public string? ParentCNIC { get; set; }

        [StringLength(1000)]
        public string? ParentUndertakingNotes { get; set; }

        [StringLength(100)]
        public string? ReinstatedByStaffId { get; set; }

        [StringLength(100)]
        public string? ReinstatedByUserId { get; set; }

        public DateTime? ReinstatedDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal FinePaidAmount { get; set; } = 0.00m;
    }
}
