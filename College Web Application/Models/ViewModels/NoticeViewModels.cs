using System.ComponentModel.DataAnnotations;
using College_Web_Application.Models;
using Microsoft.AspNetCore.Http;

namespace College_Web_Application.Models.ViewModels
{
    public class NoticeListViewModel
    {
        public List<Notice> Notices { get; set; } = new();
        public string SelectedCategory { get; set; } = "All";
        public string? SearchKeyword { get; set; }
        public int TotalCount { get; set; }
        public List<string> Categories { get; set; } = new() { "All", "Admission", "Exam", "Scholarship", "TimeTable", "General" };
    }

    public class CreateNoticeViewModel
    {
        [Required]
        [StringLength(200, MinimumLength = 5)]
        [Display(Name = "Notice Title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Category")]
        public string Category { get; set; } = "General";

        [Required]
        [Display(Name = "Detailed Description")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Attachment (PDF File, Max 5MB)")]
        public IFormFile? PdfFile { get; set; }

        [Display(Name = "Pin to Top / Live Scrolling Ticker")]
        public bool IsPinned { get; set; } = false;
    }
}
