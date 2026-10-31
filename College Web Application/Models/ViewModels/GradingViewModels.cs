using College_Web_Application.Models;

namespace College_Web_Application.Models.ViewModels
{
    public class AssignedCourseItemViewModel
    {
        public int AssignmentId { get; set; }
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public int CreditHours { get; set; }
        public string ProgramTitle { get; set; } = string.Empty;
        public string Level { get; set; } = "BS";
        public int Semester { get; set; }
        public string Section { get; set; } = "A";
        public string AcademicSession { get; set; } = "Fall 2026";
        public int EnrolledStudentsCount { get; set; }
        public int GradedCount { get; set; }
        public ExamSubmissionStatus Status { get; set; } = ExamSubmissionStatus.Draft;
        public string? ControllerRemarks { get; set; }
        public bool IsLocked => Status == ExamSubmissionStatus.SubmittedToController || Status == ExamSubmissionStatus.ApprovedByController;
    }

    public class EnterMarksViewModel
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public int CreditHours { get; set; }
        public string ProgramTitle { get; set; } = string.Empty;
        public string ProgramLevel { get; set; } = "BS"; // BS or HSSC
        public int Semester { get; set; }
        public string Section { get; set; } = "A";
        public string AcademicSession { get; set; } = "Fall 2026";
        public string TeacherName { get; set; } = string.Empty;
        public ExamSubmissionStatus Status { get; set; } = ExamSubmissionStatus.Draft;
        public string? ControllerRemarks { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public bool IsLocked => Status == ExamSubmissionStatus.SubmittedToController || Status == ExamSubmissionStatus.ApprovedByController;

        public List<StudentMarkEntryItem> StudentMarks { get; set; } = new();
    }

    public class StudentMarkEntryItem
    {
        public int StudentId { get; set; }
        public string RollNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string FatherName { get; set; } = string.Empty;
        public decimal MidTermMarks { get; set; } // Max 30
        public decimal SessionalMarks { get; set; } // Max 20
        public decimal FinalMarks { get; set; } // Max 50
        public decimal TotalMarks { get; set; } // Max 100
        public string Grade { get; set; } = "F";
        public decimal GradePoints { get; set; } = 0.00m;
        public AcademicStatus AcademicStatus { get; set; } = AcademicStatus.Active;

        // BS 75% Attendance Compliance Properties
        public decimal AttendancePercentage { get; set; } = 100.00m;
        public bool IsDebarredForShortAttendance { get; set; } = false;
        public bool IsCondoned { get; set; } = false;
        public string AttendanceBadgeClass { get; set; } = "bg-success";
        public string AttendanceStatusText { get; set; } = "75%+ (Eligible)";
    }
}
