using College_Web_Application.Data;
using College_Web_Application.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace College_Web_Application.Controllers
{
    public class DepartmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DepartmentsController> _logger;

        public DepartmentsController(
            ApplicationDbContext context,
            ILogger<DepartmentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Departments
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var departments = await _context.Departments
                .AsNoTracking()
                .Include(d => d.Programs)
                .Include(d => d.FacultyMembers.Where(f => f.IsActive))
                .OrderBy(d => d.Id)
                .ToListAsync();

            return View(departments);
        }

        // GET: /Departments/Details/1 or /Departments/Details?code=CS
        [HttpGet]
        public async Task<IActionResult> Details(int? id, string? code)
        {
            if (!id.HasValue && string.IsNullOrWhiteSpace(code))
            {
                return RedirectToAction(nameof(Index));
            }

            IQueryable<Department> query = _context.Departments
                .AsNoTracking()
                .Include(d => d.Programs)
                .Include(d => d.FacultyMembers.Where(f => f.IsActive))
                .Include(d => d.GalleryItems)
                .Include(d => d.Resources);

            Department? department;
            if (id.HasValue)
            {
                department = await query.FirstOrDefaultAsync(d => d.Id == id.Value);
            }
            else
            {
                department = await query.FirstOrDefaultAsync(d => d.Code.ToLower() == code!.ToLower());
            }

            if (department == null)
            {
                return NotFound();
            }

            // Order navigation collections for presentation
            department.FacultyMembers = department.FacultyMembers
                .OrderByDescending(f => f.BPSGrade)
                .ThenBy(f => f.FullName)
                .ToList();

            department.GalleryItems = department.GalleryItems
                .OrderByDescending(g => g.UploadedAt)
                .ToList();

            department.Resources = department.Resources
                .OrderBy(r => r.Category)
                .ThenBy(r => r.TargetSemester)
                .ThenBy(r => r.Title)
                .ToList();

            return View(department);
        }
    }
}
