using College_Web_Application.Models;

namespace College_Web_Application.Models.ViewModels
{
    public class MarkAttendanceViewModel
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public string ProgramTitle { get; set; } = string.Empty;
        public string ProgramLevel { get; set; } = "HSSC"; // HSSC or BS
        public int Semester { get; set; } = 1;
        public string Section { get; set; } = "A";
        public string AcademicSession { get; set; } = "Fall 2026";
        public DateTime AttendanceDate { get; set; } = DateTime.UtcNow.Date;
        public string TeacherName { get; set; } = string.Empty;

        public List<StudentAttendanceItemViewModel> Students { get; set; } = new();
        public List<AssignedCourseOptionItem> AssignedCourses { get; set; } = new();
    }

    public class AssignedCourseOptionItem
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public string Section { get; set; } = "A";
        public string ProgramLevel { get; set; } = "HSSC";
        public string AcademicSession { get; set; } = "Fall 2026";
    }

    public class StudentAttendanceItemViewModel
    {
        public int StudentId { get; set; }
        public string RollNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string FatherName { get; set; } = string.Empty;
        public AcademicStatus AcademicStatus { get; set; } = AcademicStatus.Active;
        public int ConsecutiveAbsentDays { get; set; } = 0;
        public string Status { get; set; } = "Present"; // Present, Absent, Leave
        public bool IsRusticated => AcademicStatus == AcademicStatus.Rusticated_ParentRequired || AcademicStatus == AcademicStatus.StruckOff;
    }
}
