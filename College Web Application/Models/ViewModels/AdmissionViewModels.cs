using System.ComponentModel.DataAnnotations;
using College_Web_Application.Models;
using Microsoft.AspNetCore.Http;

namespace College_Web_Application.Models.ViewModels
{
    public class AdmissionApplyViewModel
    {
        // Personal Details
        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(150)]
        [Display(Name = "Applicant Full Name")]
        public string ApplicantName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Father's Name is required.")]
        [StringLength(150)]
        [Display(Name = "Father's Name")]
        public string FatherName { get; set; } = string.Empty;

        [Required(ErrorMessage = "CNIC or Form-B number is required.")]
        [RegularExpression(@"^\d{5}-\d{7}-\d{1}$", ErrorMessage = "CNIC format must be 15602-1234567-1")]
        [Display(Name = "CNIC / Form-B Number")]
        public string CNIC { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of Birth is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; } = DateTime.UtcNow.AddYears(-18);

        [Required]
        [Display(Name = "Gender")]
        public string Gender { get; set; } = "Male";

        [Display(Name = "Blood Group")]
        public string? BloodGroup { get; set; } = "B+";

        [Required(ErrorMessage = "Contact mobile number is required.")]
        [RegularExpression(@"^03\d{2}-\d{7}$", ErrorMessage = "Mobile format must be 0300-1234567")]
        [Display(Name = "Applicant Mobile No")]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "Guardian / Father Mobile No")]
        public string? GuardianPhone { get; set; }

        [Required]
        [Display(Name = "Domicile District")]
        public string DomicileDistrict { get; set; } = "Swat";

        [Required]
        [Display(Name = "Tehsil")]
        public string Tehsil { get; set; } = "Bahrain / Madyan";

        [Required]
        [StringLength(255)]
        [Display(Name = "Complete Postal Address")]
        public string PostalAddress { get; set; } = string.Empty;

        // SSC / Matric Record
        [Required]
        [Display(Name = "SSC Board")]
        public string SscBoard { get; set; } = "BISE Swat";

        [Required]
        [Display(Name = "SSC Roll Number")]
        public string SscRollNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "SSC Passing Year")]
        public int SscPassingYear { get; set; } = 2024;

        [Required]
        [Range(100, 1200, ErrorMessage = "Obtained marks must be valid.")]
        [Display(Name = "Matric Obtained Marks")]
        public decimal SscObtainedMarks { get; set; }

        [Required]
        [Range(500, 1200, ErrorMessage = "Total marks must be valid.")]
        [Display(Name = "Matric Total Marks")]
        public decimal SscTotalMarks { get; set; } = 1100.00m;

        // HSSC / Intermediate Record (Required if applying for BS 4-Year)
        [Display(Name = "HSSC Board")]
        public string? HsscBoard { get; set; } = "BISE Swat";

        [Display(Name = "HSSC Roll Number")]
        public string? HsscRollNumber { get; set; }

        [Display(Name = "HSSC Passing Year")]
        public int? HsscPassingYear { get; set; } = 2026;

        [Display(Name = "HSSC Obtained Marks")]
        public decimal? HsscObtainedMarks { get; set; }

        [Display(Name = "HSSC Total Marks")]
        public decimal? HsscTotalMarks { get; set; } = 1100.00m;

        // Program & Quota Choice
        [Required(ErrorMessage = "Please select your primary desired program.")]
        [Display(Name = "Desired Academic Program")]
        public int AppliedProgramId { get; set; }

        [Display(Name = "Second Choice Program (Optional)")]
        public int? SecondChoiceProgramId { get; set; }

        [Required]
        [Display(Name = "Admission Quota")]
        public string QuotaCategory { get; set; } = "Open Merit";

        [Display(Name = "Are you a Hafiz-e-Quran? (+20 marks)")]
        public bool IsHafizQuran { get; set; } = false;

        // File Uploads
        [Display(Name = "Passport Size Photograph (JPG/PNG, max 1MB)")]
        public IFormFile? PassportPhoto { get; set; }

        [Display(Name = "DMC / Marks Sheet Scan (JPG/PNG/PDF, max 2MB)")]
        public IFormFile? DmcScanFile { get; set; }

        [Display(Name = "Domicile or B-Form Scan")]
        public IFormFile? DomicileOrBFormFile { get; set; }

        // Dropdown collections
        public List<AcademicProgram> Programs { get; set; } = new();
    }

    public class ApplicationStatusViewModel
    {
        public string TrackingNumber { get; set; } = string.Empty;
        public string ApplicantName { get; set; } = string.Empty;
        public string FatherName { get; set; } = string.Empty;
        public string CNIC { get; set; } = string.Empty;
        public string AppliedProgramTitle { get; set; } = string.Empty;
        public string QuotaCategory { get; set; } = string.Empty;
        public decimal MeritScore { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime AppliedDate { get; set; }
        public decimal MatricObtainedMarks { get; set; }
        public decimal MatricTotalMarks { get; set; }
        public decimal? HsscObtainedMarks { get; set; }
        public decimal? HsscTotalMarks { get; set; }
        public bool IsHafizQuran { get; set; }
    }

    public class MeritListFilterViewModel
    {
        public List<AdmissionApplication> Applications { get; set; } = new();
        public int? ProgramId { get; set; }
        public string QuotaCategory { get; set; } = "All";
        public string? SearchQuery { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public decimal CutOffMeritPercentage { get; set; }
        public List<AcademicProgram> Programs { get; set; } = new();
    }
}
