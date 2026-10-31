using College_Web_Application.Models;

namespace College_Web_Application.Models.ViewModels
{
    public class ControllerDashboardViewModel
    {
        public int SelectedProgramId { get; set; }
        public int SelectedSemester { get; set; }
        public List<AcademicProgram> Programs { get; set; } = new();
        public List<CourseAwardListStatusItem> CourseStatuses { get; set; } = new();
        public List<StudentSummaryDmcItem> StudentsInSemester { get; set; } = new();
        public bool AllCoursesApproved { get; set; }
    }

    public class CourseAwardListStatusItem
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int CreditHours { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public string Section { get; set; } = "A";
        public string AcademicSession { get; set; } = "Fall 2026";
        public int TotalStudents { get; set; }
        public int GradedStudents { get; set; }
        public ExamSubmissionStatus Status { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string? ControllerRemarks { get; set; }
    }

    public class StudentSummaryDmcItem
    {
        public int StudentId { get; set; }
        public string RollNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string FatherName { get; set; } = string.Empty;
        public int CompletedCoursesCount { get; set; }
        public int TotalSemesterCoursesCount { get; set; }
        public decimal SGPA { get; set; }
        public decimal CGPA { get; set; }
        public bool CanGenerateDmc => CompletedCoursesCount == TotalSemesterCoursesCount && TotalSemesterCoursesCount > 0;
    }

    public class OfficialDmcViewModel
    {
        public string SerialNumber { get; set; } = string.Empty;
        public int StudentId { get; set; }
        public string RollNumber { get; set; } = string.Empty;
        public string RegistrationNo { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string FatherName { get; set; } = string.Empty;
        public string ProgramTitle { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public int Semester { get; set; }
        public string AcademicSession { get; set; } = "Fall 2026";
        public DateTime IssueDate { get; set; } = DateTime.UtcNow;
        public string QRVerificationPayload { get; set; } = string.Empty;

        public bool IsFullyVerified { get; set; } = true;
        public List<string> UnverifiedCourseCodes { get; set; } = new();

        public List<DmcCourseGradeItem> Courses { get; set; } = new();

        public int TotalCreditHours { get; set; }
        public decimal TotalQualityPoints { get; set; }
        public decimal SGPA { get; set; }
        public decimal CGPA { get; set; }
        public string Remarks { get; set; } = "PASSED";
    }

    public class DmcCourseGradeItem
    {
        public string CourseCode { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public int CreditHours { get; set; }
        public decimal MidTermMarks { get; set; }
        public decimal SessionalMarks { get; set; }
        public decimal FinalMarks { get; set; }
        public decimal TotalMarks { get; set; }
        public string Grade { get; set; } = "F";
        public decimal GradePoints { get; set; }
        public decimal QualityPoints => CreditHours * GradePoints;
        public ExamSubmissionStatus Status { get; set; }
    }

    public class AttendanceShortageListViewModel
    {
        public int SelectedProgramId { get; set; }
        public int SelectedSemester { get; set; }
        public List<AcademicProgram> Programs { get; set; } = new();
        public List<AttendanceShortageItemViewModel> ShortageStudents { get; set; } = new();
        public int TotalEvaluated { get; set; }
        public int TotalEligible { get; set; }
        public int TotalWarningCondonationZone { get; set; }
        public int TotalDebarred { get; set; }
        public string ProgramTitle { get; set; } = string.Empty;
    }

    public class AttendanceShortageItemViewModel
    {
        public int StudentId { get; set; }
        public string RollNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string FatherName { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public int Semester { get; set; }
        public int TotalConducted { get; set; }
        public int TotalPresent { get; set; }
        public int TotalLeave { get; set; }
        public int TotalAbsent { get; set; }
        public decimal AttendancePercentage { get; set; }
        public College_Web_Application.Services.AttendanceEligibilityStatus Status { get; set; }
        public int ClassesNeededToReach75 { get; set; }
        public bool IsCondonedByPrincipal { get; set; }
        public string? CondonationChallan { get; set; }
        public string? CondonationReason { get; set; }
        public string WhatsAppUrl { get; set; } = string.Empty;
    }
}
