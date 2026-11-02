using College_Web_Application.Models;
using College_Web_Application.Services;

namespace College_Web_Application.Models.ViewModels
{
    public class StudentPortalDashboardViewModel
    {
        public StudentProfile Student { get; set; } = null!;
        public List<CourseAttendanceEvaluation> AttendanceEvaluations { get; set; } = new();
        public List<ExamResult> SemesterResults { get; set; } = new();
        public List<FeeChallan> RecentFeeChallans { get; set; } = new();
        public List<Notice> CollegeAnnouncements { get; set; } = new();
        public List<StudentTimetableItem> WeeklySchedule { get; set; } = new();
        
        public bool IsAnyCourseDebarred { get; set; }
        public bool CanDownloadRollNoSlip { get; set; }
        public int EligibleCoursesCount { get; set; }
        public int WarningCoursesCount { get; set; }
        public int DebarredCoursesCount { get; set; }
        public decimal OverallAttendancePercentage { get; set; }
        public decimal CurrentSgpa { get; set; }
        public int TotalCreditHoursCompleted { get; set; }
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
        public string VerificationQrUrl { get; set; } = string.Empty;
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

    public class StudentDmcViewModel
    {
        public StudentProfile Student { get; set; } = null!;
        public int Semester { get; set; }
        public string AcademicSession { get; set; } = "Fall 2026";
        public DateTime IssueDate { get; set; } = DateTime.UtcNow;
        public List<DmcCourseResultItem> CourseResults { get; set; } = new();
        public decimal TotalCreditHours { get; set; }
        public decimal TotalQualityPoints { get; set; }
        public decimal Sgpa { get; set; }
        public decimal Cgpa { get; set; }
        public string ControllerName { get; set; } = "Office of the Controller of Examinations";
        public string VerificationQrUrl { get; set; } = string.Empty;
    }

    public class DmcCourseResultItem
    {
        public string CourseCode { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public int CreditHours { get; set; }
        public decimal MidTermMarks { get; set; }
        public decimal SessionalMarks { get; set; }
        public decimal FinalMarks { get; set; }
        public decimal TotalMarks { get; set; }
        public string Grade { get; set; } = "A";
        public decimal GradePoints { get; set; } = 4.00m;
        public decimal QualityPoints => CreditHours * GradePoints;
        public string Remarks { get; set; } = "Pass";
    }

    public class StudentFeeListViewModel
    {
        public StudentProfile Student { get; set; } = null!;
        public List<FeeChallan> Challans { get; set; } = new();
        public decimal TotalBilled { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalOutstanding { get; set; }
    }

    public class StudentTimetableItem
    {
        public string DayOfWeek { get; set; } = "Monday";
        public string CourseCode { get; set; } = "CS-301";
        public string CourseTitle { get; set; } = "Database Systems";
        public string TimeSlot { get; set; } = "09:00 AM - 10:30 AM";
        public string Room { get; set; } = "Lab-1 (Block-A)";
        public string Instructor { get; set; } = "Mr. Irfan Ullah";
    }
}
