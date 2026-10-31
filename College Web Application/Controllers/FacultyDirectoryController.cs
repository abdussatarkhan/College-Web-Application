using College_Web_Application.Data;
using College_Web_Application.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace College_Web_Application.Controllers
{
    public class FacultyDirectoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<FacultyDirectoryController> _logger;

        private const string CacheKey = "GDC_FacultyDirectory_Data";

        public FacultyDirectoryController(
            ApplicationDbContext context,
            IMemoryCache cache,
            ILogger<FacultyDirectoryController> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        // GET: /FacultyDirectory
        public async Task<IActionResult> Index(int? departmentId, string? search)
        {
            var departments = await _context.Departments
                .AsNoTracking()
                .OrderBy(d => d.Id)
                .ToListAsync();

            var staffQuery = _context.StaffProfiles
                .AsNoTracking()
                .Include(s => s.Department)
                .Where(s => s.IsActive && s.DepartmentId != null) // Teaching faculty
                .AsQueryable();

            if (departmentId.HasValue && departmentId.Value > 0)
            {
                staffQuery = staffQuery.Where(s => s.DepartmentId == departmentId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.Trim().ToLower();
                staffQuery = staffQuery.Where(s =>
                    s.FullName.ToLower().Contains(q) ||
                    s.Designation.ToLower().Contains(q) ||
                    s.Qualification.ToLower().Contains(q) ||
                    (s.Department != null && s.Department.Name.ToLower().Contains(q)));
            }

            var facultyMembers = await staffQuery
                .OrderByDescending(s => s.BPSGrade)
                .ThenBy(s => s.FullName)
                .ToListAsync();

            // Group by department
            var grouped = departments
                .Where(d => !departmentId.HasValue || d.Id == departmentId.Value)
                .Select(d => new DepartmentFacultyGroup
                {
                    Department = d,
                    FacultyMembers = facultyMembers.Where(f => f.DepartmentId == d.Id).ToList()
                })
                .Where(g => g.FacultyMembers.Any() || !string.IsNullOrEmpty(search) || departmentId.HasValue)
                .ToList();

            var viewModel = new FacultyDirectoryViewModel
            {
                DepartmentGroups = grouped,
                Departments = departments,
                SelectedDepartmentId = departmentId,
                SearchQuery = search,
                TotalFacultyCount = facultyMembers.Count
            };

            return View(viewModel);
        }
    }
}
