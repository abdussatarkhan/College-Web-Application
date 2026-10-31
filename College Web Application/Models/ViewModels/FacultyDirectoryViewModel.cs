using College_Web_Application.Models;

namespace College_Web_Application.Models.ViewModels
{
    public class FacultyDirectoryViewModel
    {
        public List<DepartmentFacultyGroup> DepartmentGroups { get; set; } = new();
        public List<Department> Departments { get; set; } = new();
        public int? SelectedDepartmentId { get; set; }
        public string? SearchQuery { get; set; }
        public int TotalFacultyCount { get; set; }
    }

    public class DepartmentFacultyGroup
    {
        public Department Department { get; set; } = new();
        public List<StaffProfile> FacultyMembers { get; set; } = new();
    }
}
