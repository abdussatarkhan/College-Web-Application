using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace College_Web_Application.Models
{
    public class DepartmentResource
    {
        public int Id { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        [ForeignKey(nameof(DepartmentId))]
        public virtual Department? Department { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = "Syllabus"; // Syllabus, Book, CourseOutline, LabManual

        public int? TargetSemester { get; set; } // 1, 2, 3, etc. or null for all

        [Required]
        [StringLength(255)]
        public string FileUrl { get; set; } = string.Empty;

        [StringLength(50)]
        public string FileFormat { get; set; } = "PDF";

        public long FileSizeBytes { get; set; }

        [StringLength(200)]
        public string? AuthorOrPublisher { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string? UploadedBy { get; set; }
    }
}
