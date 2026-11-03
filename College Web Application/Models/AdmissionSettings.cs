using System.ComponentModel.DataAnnotations;

namespace College_Web_Application.Models
{
    public class AdmissionSettings
    {
        public int Id { get; set; } = 1;

        public bool IsBsAdmissionsOpen { get; set; } = true;

        public bool IsFscAdmissionsOpen { get; set; } = true;

        [Required]
        [StringLength(100)]
        public string AdmissionSession { get; set; } = "Academic Session 2026-2027";

        [StringLength(500)]
        public string? ClosureNotice { get; set; } = "Online admissions for this program level are currently closed by the Principal's Office. Stay tuned for subsequent merit lists or next cycle.";

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string? UpdatedBy { get; set; } = "Principal Office";
    }
}
