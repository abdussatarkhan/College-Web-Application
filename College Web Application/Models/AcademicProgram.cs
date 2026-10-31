using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace College_Web_Application.Models
{
    public enum ProgramLevel
    {
        HSSC_FSc = 0,
        Undergraduate_BS = 1
    }

    public class AcademicProgram
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        public ProgramLevel ProgramLevel { get; set; } = ProgramLevel.Undergraduate_BS;

        [Required]
        [StringLength(20)]
        public string Level
        {
            get => ProgramLevel == ProgramLevel.HSSC_FSc ? "HSSC" : "BS";
            set
            {
                if (value == "HSSC" || value == "HSSC_FSc")
                    ProgramLevel = ProgramLevel.HSSC_FSc;
                else
                    ProgramLevel = ProgramLevel.Undergraduate_BS;
            }
        }

        public int DurationYears { get; set; } = 4; // 2 for HSSC, 4 for BS

        public int TotalSemesters { get; set; } = 8; // 4 for HSSC, 8 for BS

        [Required]
        public int DepartmentId { get; set; }

        [ForeignKey(nameof(DepartmentId))]
        public virtual Department? Department { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal FeePerSemester { get; set; } = 4500.00m;

        public virtual ICollection<StudentProfile> Students { get; set; } = new List<StudentProfile>();
        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
        public virtual ICollection<AdmissionApplication> Applications { get; set; } = new List<AdmissionApplication>();
    }
}
