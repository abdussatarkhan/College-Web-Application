using College_Web_Application.Models;

namespace College_Web_Application.Models.ViewModels
{
    public class HomeViewModel
    {
        public List<Notice> PinnedNotices { get; set; } = new();
        public List<AcademicProgram> IntermediatePrograms { get; set; } = new();
        public List<AcademicProgram> UndergraduatePrograms { get; set; } = new();
        public List<Department> Departments { get; set; } = new();
        public List<CampusNewsItem> CampusNews { get; set; } = new();
        public List<AlumniSpotlightItem> AlumniSpotlights { get; set; } = new();

        public int TotalStudents { get; set; } = 5200;
        public int TotalFaculty { get; set; } = 110;
        public int TotalPrograms { get; set; } = 10;
        public int YearsOfExcellence { get; set; } = 20; // 2006 to 2026
        public int DisciplinesCount { get; set; } = 8;
    }

    public class CampusNewsItem
    {
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = "Academic";
        public DateTime PublishedDate { get; set; }
        public string Summary { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string IconClass { get; set; } = "bi-newspaper";
    }

    public class AlumniSpotlightItem
    {
        public string FullName { get; set; } = string.Empty;
        public string Degree { get; set; } = string.Empty;
        public int PassingYear { get; set; }
        public string CurrentRole { get; set; } = string.Empty;
        public string Organization { get; set; } = string.Empty;
        public string Quote { get; set; } = string.Empty;
        public string BadgeClass { get; set; } = "bg-success";
        public string PhotoUrl { get; set; } = string.Empty;
    }
}
