using System.ComponentModel.DataAnnotations;

namespace College_Web_Application.Models
{
    public class Notice
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = "General"; // Admission, Exam, General, Scholarship

        [Required]
        public string Description { get; set; } = string.Empty;

        [StringLength(255)]
        public string? AttachmentPdfUrl { get; set; }

        public DateTime PublishedDate { get; set; } = DateTime.UtcNow;

        public bool IsPinned { get; set; } = false;
    }
}
