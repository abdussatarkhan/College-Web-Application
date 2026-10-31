using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace College_Web_Application.Models
{
    public class AdmissionApplication
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string TrackingNumber { get; set; } = string.Empty; // Unique Index (e.g. GDCM-ADM-2026-0001)

        [Required]
        [StringLength(150)]
        public string ApplicantName { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string FatherName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string CNIC { get; set; } = string.Empty; // 15602-xxxxxxx-x

        [Required]
        [StringLength(20)]
        public string Gender { get; set; } = "Male";

        [StringLength(10)]
        public string? BloodGroup { get; set; }

        public DateTime DateOfBirth { get; set; }

        [Required]
        [StringLength(25)]
        public string Phone { get; set; } = string.Empty;

        [StringLength(25)]
        public string? GuardianPhone { get; set; }

        [Required]
        [StringLength(100)]
        public string DomicileDistrict { get; set; } = "Swat";

        [Required]
        [StringLength(100)]
        public string Tehsil { get; set; } = "Bahrain / Madyan";

        [Required]
        [StringLength(255)]
        public string PostalAddress { get; set; } = string.Empty;

        // SSC / Matric Details
        [Required]
        [StringLength(100)]
        public string SscBoard { get; set; } = "BISE Swat";

        [Required]
        [StringLength(50)]
        public string SscRollNumber { get; set; } = string.Empty;

        public int SscPassingYear { get; set; } = 2026;

        [Column(TypeName = "decimal(6,2)")]
        public decimal MatricObtainedMarks { get; set; }

        [Column(TypeName = "decimal(6,2)")]
        public decimal MatricTotalMarks { get; set; } = 1100.00m;

        // HSSC / Intermediate Details (For BS applicants)
        [StringLength(100)]
        public string? HsscBoard { get; set; }

        [StringLength(50)]
        public string? HsscRollNumber { get; set; }

        public int? HsscPassingYear { get; set; }

        [Column(TypeName = "decimal(6,2)")]
        public decimal? HsscObtainedMarks { get; set; }

        [Column(TypeName = "decimal(6,2)")]
        public decimal? HsscTotalMarks { get; set; }

        public bool IsHafizQuran { get; set; } = false;

        [Column(TypeName = "decimal(5,2)")]
        public decimal HafizMarks { get; set; } = 0.00m; // 20 marks if Hafiz

        [Column(TypeName = "decimal(6,2)")]
        public decimal MeritScore { get; set; } = 0.00m; // Calculated percentage/aggregate

        [Required]
        public int AppliedProgramId { get; set; }

        [ForeignKey(nameof(AppliedProgramId))]
        public virtual AcademicProgram? AppliedProgram { get; set; }

        public int? SecondChoiceProgramId { get; set; }

        [ForeignKey(nameof(SecondChoiceProgramId))]
        public virtual AcademicProgram? SecondChoiceProgram { get; set; }

        [Required]
        [StringLength(50)]
        public string QuotaCategory { get; set; } = "Open Merit"; // Open Merit, Sports, Disabled, Hafiz-e-Quran

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Verified, MeritListed, Admitted, Rejected

        public DateTime AppliedDate { get; set; } = DateTime.UtcNow;

        [StringLength(255)]
        public string? PassportPhotoPath { get; set; }

        [StringLength(255)]
        public string? DmcScanPath { get; set; }

        [StringLength(255)]
        public string? DomicileOrBFormPath { get; set; }
    }
}
