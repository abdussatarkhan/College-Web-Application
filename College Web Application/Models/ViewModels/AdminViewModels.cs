using System.ComponentModel.DataAnnotations;
using College_Web_Application.Models;

namespace College_Web_Application.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int HsscStudentsCount { get; set; }
        public int BsStudentsCount { get; set; }
        public int TotalFaculty { get; set; }
        public Dictionary<string, int> StaffGradeBreakdown { get; set; } = new();
        public decimal TotalBilledFee { get; set; }
        public decimal TotalCollectedFee { get; set; }
        public int DefaultersCount { get; set; }
        public decimal DefaultersAmount { get; set; }
        public decimal TodayAttendancePercentage { get; set; }
        public int PendingApplicationsCount { get; set; }
        public List<DepartmentEnrollmentStat> DepartmentEnrollments { get; set; } = new();
        public List<AdmissionApplication> RecentApplications { get; set; } = new();
        public List<FeeChallan> RecentFeePayments { get; set; } = new();
        public List<StudentProfile> RecentEnrolledStudents { get; set; } = new();
    }

    public class DepartmentEnrollmentStat
    {
        public string DepartmentName { get; set; } = string.Empty;
        public string DepartmentCode { get; set; } = string.Empty;
        public int StudentCount { get; set; }
    }

    public class StudentListViewModel
    {
        public List<StudentProfile> Students { get; set; } = new();
        public string? SearchQuery { get; set; }
        public int? DepartmentId { get; set; }
        public int? ProgramId { get; set; }
        public int? Semester { get; set; }
        public string? Section { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public List<Department> Departments { get; set; } = new();
        public List<AcademicProgram> Programs { get; set; } = new();
    }

    public class EnrollApplicantViewModel
    {
        [Required]
        public int ApplicationId { get; set; }

        public string ApplicantName { get; set; } = string.Empty;
        public string FatherName { get; set; } = string.Empty;
        public string CNIC { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public int ProgramId { get; set; }
        public string ProgramTitle { get; set; } = string.Empty;
        public decimal MeritScore { get; set; }

        [Required]
        [Display(Name = "Generated Roll Number")]
        public string RollNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "University / Board Registration No")]
        public string RegistrationNo { get; set; } = string.Empty;

        [Required]
        public string Section { get; set; } = "A";

        [Required]
        public int CurrentSemester { get; set; } = 1;
    }

    public class BatchPromotionViewModel
    {
        [Required]
        [Display(Name = "Academic Program")]
        public int ProgramId { get; set; }

        [Required]
        [Display(Name = "Current Semester")]
        public int CurrentSemester { get; set; }

        [Required]
        [Display(Name = "Promote to Next Semester")]
        public int NextSemester { get; set; }

        public string? Section { get; set; }
        public int EligibleStudentsCount { get; set; }
    }

    public class FeeChallanListViewModel
    {
        public List<FeeChallan> Challans { get; set; } = new();
        public string? SearchQuery { get; set; }
        public int? ProgramId { get; set; }
        public int? Semester { get; set; }
        public string? StatusFilter { get; set; } = "All"; // All, Paid, Unpaid
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public List<AcademicProgram> Programs { get; set; } = new();
    }

    public class BulkChallanGenerateViewModel
    {
        [Required]
        [Display(Name = "Academic Program")]
        public int ProgramId { get; set; }

        [Required]
        [Display(Name = "Semester")]
        public int Semester { get; set; } = 1;

        [Required]
        [Display(Name = "Tuition Fee (PKR)")]
        public decimal TuitionFee { get; set; } = 4500.00m;

        [Display(Name = "Admission Fee (PKR)")]
        public decimal AdmissionFee { get; set; } = 0.00m;

        [Display(Name = "Exam Fee (PKR)")]
        public decimal ExamFee { get; set; } = 1500.00m;

        [Display(Name = "Library Fund (PKR)")]
        public decimal LibraryFund { get; set; } = 500.00m;

        [Display(Name = "Lab / Computer Fund (PKR)")]
        public decimal LabFund { get; set; } = 500.00m;

        [Required]
        [Display(Name = "Due Date")]
        public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(20);
    }

    public class FeePaymentVerificationViewModel
    {
        [Required]
        [Display(Name = "Challan Number")]
        public string ChallanNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Bank Reference / Scroll Number")]
        public string BankReference { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Payment Deposit Date")]
        public DateTime PaidDate { get; set; } = DateTime.UtcNow;
    }
}
