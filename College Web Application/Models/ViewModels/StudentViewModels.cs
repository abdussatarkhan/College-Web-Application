using College_Web_Application.Models;
using College_Web_Application.Services;

namespace College_Web_Application.Models.ViewModels
{
    public class StudentPortalDashboardViewModel
    {
        public StudentProfile Student { get; set; } = null!;
        public List<CourseAttendanceEvaluation> AttendanceEvaluations { get; set; } = new();
        public List<ExamResult> SemesterResults { get; set; } = new();
        public bool IsAnyCourseDebarred { get; set; }
        public bool CanDownloadRollNoSlip { get; set; }
        public int EligibleCoursesCount { get; set; }
        public int WarningCoursesCount { get; set; }
        public int DebarredCoursesCount { get; set; }
        public decimal OverallAttendancePercentage { get; set; }
    }

    public class StudentRollNoSlipViewModel
    {
        public string SlipNumber { get; set; } = string.Empty;
        public StudentProfile Student { get; set; } = null!;
        public int Semester { get; set; }
        public string AcademicSession { get; set; } = "Fall 2026";
        public DateTime ExamCommencementDate { get; set; } = DateTime.UtcNow.AddDays(14);
        public string ExaminationCenter { get; set; } = "Main Examination Hall, Block-A, GDC Madyan Swat";
        public List<RollNoSlipCourseItem> Courses { get; set; } = new();
        public bool CanAppearInExams { get; set; } // At least 1 course eligible
    }

    public class RollNoSlipCourseItem
    {
        public string CourseCode { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public int CreditHours { get; set; }
        public decimal AttendancePercentage { get; set; }
        public bool IsEligible { get; set; }
        public bool IsCondoned { get; set; }
        public string StatusBadge { get; set; } = "ELIGIBLE";
        public string Remarks { get; set; } = string.Empty;
        public string ExamDate { get; set; } = "TBD";
        public string ExamTime { get; set; } = "09:00 AM - 12:00 PM";
    }
}
