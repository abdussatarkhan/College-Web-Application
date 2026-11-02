using College_Web_Application.Models;
using System.ComponentModel.DataAnnotations;

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
        public List<CampusEventItem> UpcomingEvents { get; set; } = new();

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

    public class CampusEventItem
    {
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = "Academic";
        public DateTime EventDate { get; set; }
        public string Time { get; set; } = "09:00 AM - 01:00 PM";
        public string Venue { get; set; } = "Main Auditorium / Exam Hall";
        public string Description { get; set; } = string.Empty;
        public string BadgeColor { get; set; } = "bg-success";
    }

    public class DownloadableDocumentItem
    {
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = "Forms";
        public string FileSize { get; set; } = "1.2 MB";
        public string FileType { get; set; } = "PDF";
        public string Description { get; set; } = string.Empty;
        public string DownloadUrl { get; set; } = "#";
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    public class ContactInquiryViewModel
    {
        [Required(ErrorMessage = "Name is required.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please provide a valid email.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Subject is required.")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Message is required.")]
        [StringLength(1000, MinimumLength = 10)]
        public string Message { get; set; } = string.Empty;
    }
}
