using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace College_Web_Application.Models
{
    public class FeeChallan
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string ChallanNumber { get; set; } = string.Empty; // Unique Index (e.g. BOK-GDCM-2026-1001)

        [Required]
        public int StudentId { get; set; }

        [ForeignKey(nameof(StudentId))]
        public virtual StudentProfile? Student { get; set; }

        public int Semester { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TuitionFee { get; set; } = 3500.00m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal AdmissionFee { get; set; } = 1000.00m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal ExamFee { get; set; } = 1500.00m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal LibraryFund { get; set; } = 500.00m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal LabFund { get; set; } = 500.00m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal LateFine { get; set; } = 500.00m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; } = 7000.00m;

        public DateTime IssueDate { get; set; } = DateTime.UtcNow;

        public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(15);

        public bool IsPaid { get; set; } = false;

        public DateTime? PaidDate { get; set; }

        [StringLength(100)]
        public string? BankReference { get; set; } // BoK / NBP Branch deposit scroll number
    }
}
