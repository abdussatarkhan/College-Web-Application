using System.ComponentModel.DataAnnotations;

namespace College_Web_Application.Models
{
    public class Department
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string HODName { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public virtual ICollection<AcademicProgram> Programs { get; set; } = new List<AcademicProgram>();
        public virtual ICollection<StaffProfile> FacultyMembers { get; set; } = new List<StaffProfile>();
    }
}
