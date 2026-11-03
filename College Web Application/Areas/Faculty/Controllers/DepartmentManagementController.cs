using College_Web_Application.Data;
using College_Web_Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace College_Web_Application.Areas.Faculty.Controllers
{
    [Area("Faculty")]
    [Authorize(Roles = "HOD,Principal,SuperAdmin")]
    public class DepartmentManagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<DepartmentManagementController> _logger;

        public DepartmentManagementController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            IWebHostEnvironment env,
            ILogger<DepartmentManagementController> logger)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
            _logger = logger;
        }

        // GET: /Faculty/DepartmentManagement
        [HttpGet]
        public async Task<IActionResult> Index(int? departmentId)
        {
            var user = await _userManager.GetUserAsync(User);
            var allDepartments = await _context.Departments
                .AsNoTracking()
                .OrderBy(d => d.Id)
                .ToListAsync();

            ViewBag.AllDepartments = allDepartments;

            int targetDeptId;

            // If an explicit departmentId was passed and the user is Principal or SuperAdmin, allow selection
            if (departmentId.HasValue && (User.IsInRole("Principal") || User.IsInRole("SuperAdmin")))
            {
                targetDeptId = departmentId.Value;
            }
            else
            {
                // Try resolving HOD's assigned department from StaffProfile
                var staff = await _context.StaffProfiles
                    .FirstOrDefaultAsync(s => (user != null && s.IdentityUserId == user.Id) || (user != null && s.EmployeeCode == user.UserName));

                if (staff?.DepartmentId != null)
                {
                    targetDeptId = staff.DepartmentId.Value;
                }
                else if (departmentId.HasValue)
                {
                    targetDeptId = departmentId.Value;
                }
                else
                {
                    targetDeptId = allDepartments.FirstOrDefault()?.Id ?? 1;
                }
            }

            var department = await _context.Departments
                .Include(d => d.Programs)
                .Include(d => d.FacultyMembers)
                .Include(d => d.GalleryItems)
                .Include(d => d.Resources)
                .FirstOrDefaultAsync(d => d.Id == targetDeptId);

            if (department == null)
            {
                return NotFound("Department record not found.");
            }

            return View(department);
        }

        // POST: /Faculty/DepartmentManagement/UpdateDepartmentInfo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDepartmentInfo(int departmentId, string description, string hodName)
        {
            var dept = await _context.Departments.FindAsync(departmentId);
            if (dept == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(description)) dept.Description = description.Trim();
            if (!string.IsNullOrWhiteSpace(hodName)) dept.HODName = hodName.Trim();

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Department profile & HOD statement updated successfully.";
            return RedirectToAction(nameof(Index), new { departmentId });
        }

        // POST: /Faculty/DepartmentManagement/UploadGalleryPhoto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadGalleryPhoto(int departmentId, string title, string? caption, IFormFile? photo)
        {
            if (photo == null || photo.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a valid image file to upload.";
                return RedirectToAction(nameof(Index), new { departmentId });
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                TempData["ErrorMessage"] = "Please provide an event title or caption for the photo.";
                return RedirectToAction(nameof(Index), new { departmentId });
            }

            var allowedExts = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(photo.FileName).ToLowerInvariant();
            if (!allowedExts.Contains(ext))
            {
                TempData["ErrorMessage"] = "Invalid image format. Allowed formats: JPG, PNG, WEBP.";
                return RedirectToAction(nameof(Index), new { departmentId });
            }

            string uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "departments", departmentId.ToString(), "gallery");
            if (!Directory.Exists(uploadsDir))
            {
                Directory.CreateDirectory(uploadsDir);
            }

            string uniqueFilename = $"{Guid.NewGuid():N}{ext}";
            string filePath = Path.Combine(uploadsDir, uniqueFilename);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await photo.CopyToAsync(stream);
            }

            string imageUrl = $"/uploads/departments/{departmentId}/gallery/{uniqueFilename}";

            var galleryItem = new DepartmentGalleryItem
            {
                DepartmentId = departmentId,
                Title = title.Trim(),
                Caption = caption?.Trim(),
                ImageUrl = imageUrl,
                UploadedAt = DateTime.UtcNow,
                UploadedBy = User.Identity?.Name ?? "Department HOD"
            };

            _context.DepartmentGalleryItems.Add(galleryItem);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Photo '{title}' successfully uploaded to the Department Gallery.";
            return RedirectToAction(nameof(Index), new { departmentId });
        }

        // POST: /Faculty/DepartmentManagement/DeleteGalleryPhoto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGalleryPhoto(int photoId, int departmentId)
        {
            var item = await _context.DepartmentGalleryItems.FirstOrDefaultAsync(g => g.Id == photoId && g.DepartmentId == departmentId);
            if (item != null)
            {
                _context.DepartmentGalleryItems.Remove(item);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Photo removed from department gallery.";
            }

            return RedirectToAction(nameof(Index), new { departmentId });
        }

        // POST: /Faculty/DepartmentManagement/AddFacultyMember
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFacultyMember(
            int departmentId, 
            string fullName, 
            string designation, 
            string bpsGrade, 
            string qualification, 
            string employeeCode, 
            IFormFile? staffPhoto)
        {
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(designation))
            {
                TempData["ErrorMessage"] = "Full name and designation are required.";
                return RedirectToAction(nameof(Index), new { departmentId });
            }

            string photoUrl = "/images/crest-placeholder.png";

            if (staffPhoto != null && staffPhoto.Length > 0)
            {
                var allowedExts = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var ext = Path.GetExtension(staffPhoto.FileName).ToLowerInvariant();
                if (allowedExts.Contains(ext))
                {
                    string uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "staff");
                    if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);

                    string filename = $"{Guid.NewGuid():N}{ext}";
                    string filePath = Path.Combine(uploadsDir, filename);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await staffPhoto.CopyToAsync(stream);
                    }

                    photoUrl = $"/uploads/staff/{filename}";
                }
            }

            var staff = new StaffProfile
            {
                DepartmentId = departmentId,
                FullName = fullName.Trim(),
                Designation = designation.Trim(),
                BPSGrade = string.IsNullOrWhiteSpace(bpsGrade) ? "BPS-17" : bpsGrade.Trim(),
                Qualification = string.IsNullOrWhiteSpace(qualification) ? "MS / M.Phil" : qualification.Trim(),
                EmployeeCode = string.IsNullOrWhiteSpace(employeeCode) ? $"EMP-GDCM-{DateTime.UtcNow.Ticks % 10000:0000}" : employeeCode.Trim(),
                PhotoUrl = photoUrl,
                JoiningDate = DateTime.UtcNow,
                IsActive = true
            };

            _context.StaffProfiles.Add(staff);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Faculty member '{fullName}' successfully added to Department roster.";
            return RedirectToAction(nameof(Index), new { departmentId });
        }

        // POST: /Faculty/DepartmentManagement/DeleteFacultyMember
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFacultyMember(int staffId, int departmentId)
        {
            var staff = await _context.StaffProfiles.FirstOrDefaultAsync(s => s.Id == staffId && s.DepartmentId == departmentId);
            if (staff != null)
            {
                staff.IsActive = false;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Faculty member removed from active roster.";
            }

            return RedirectToAction(nameof(Index), new { departmentId });
        }

        // POST: /Faculty/DepartmentManagement/UploadResource
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadResource(
            int departmentId, 
            string title, 
            string category, 
            int? targetSemester, 
            string? authorOrPublisher, 
            IFormFile? pdfFile)
        {
            if (pdfFile == null || pdfFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a valid PDF file to upload.";
                return RedirectToAction(nameof(Index), new { departmentId });
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                TempData["ErrorMessage"] = "Document title is required.";
                return RedirectToAction(nameof(Index), new { departmentId });
            }

            var ext = Path.GetExtension(pdfFile.FileName).ToLowerInvariant();
            if (ext != ".pdf")
            {
                TempData["ErrorMessage"] = "Only PDF format (.pdf) is supported for syllabi and books.";
                return RedirectToAction(nameof(Index), new { departmentId });
            }

            string uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "departments", departmentId.ToString(), "resources");
            if (!Directory.Exists(uploadsDir))
            {
                Directory.CreateDirectory(uploadsDir);
            }

            string filename = $"{Guid.NewGuid():N}.pdf";
            string filePath = Path.Combine(uploadsDir, filename);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await pdfFile.CopyToAsync(stream);
            }

            string fileUrl = $"/uploads/departments/{departmentId}/resources/{filename}";

            var resource = new DepartmentResource
            {
                DepartmentId = departmentId,
                Title = title.Trim(),
                Category = string.IsNullOrWhiteSpace(category) ? "Syllabus" : category.Trim(),
                TargetSemester = targetSemester,
                AuthorOrPublisher = authorOrPublisher?.Trim(),
                FileUrl = fileUrl,
                FileFormat = "PDF",
                FileSizeBytes = pdfFile.Length,
                UploadedAt = DateTime.UtcNow,
                UploadedBy = User.Identity?.Name ?? "Department HOD"
            };

            _context.DepartmentResources.Add(resource);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"PDF resource '{title}' successfully uploaded and published.";
            return RedirectToAction(nameof(Index), new { departmentId });
        }

        // POST: /Faculty/DepartmentManagement/DeleteResource
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteResource(int resourceId, int departmentId)
        {
            var item = await _context.DepartmentResources.FirstOrDefaultAsync(r => r.Id == resourceId && r.DepartmentId == departmentId);
            if (item != null)
            {
                _context.DepartmentResources.Remove(item);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Academic resource / PDF deleted successfully.";
            }

            return RedirectToAction(nameof(Index), new { departmentId });
        }
    }
}
