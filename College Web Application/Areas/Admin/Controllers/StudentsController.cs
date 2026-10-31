using System.Text.RegularExpressions;
using College_Web_Application.Data;
using College_Web_Application.Models;
using College_Web_Application.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace College_Web_Application.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperAdmin,Principal,Clerk")]
    public class StudentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<StudentsController> _logger;

        public StudentsController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            ILogger<StudentsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Admin/Students
        public async Task<IActionResult> Index(
            string? searchQuery,
            int? departmentId,
            int? programId,
            int? semester,
            string? section,
            int page = 1)
        {
            const int pageSize = 50;
            if (page < 1) page = 1;

            var query = _context.StudentProfiles
                .AsNoTracking()
                .Include(s => s.Program)
                    .ThenInclude(p => p!.Department)
                .Where(s => s.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var q = searchQuery.Trim().ToLower();
                query = query.Where(s =>
                    s.FullName.ToLower().Contains(q) ||
                    s.RollNumber.ToLower().Contains(q) ||
                    s.CNIC.Contains(q) ||
                    s.RegistrationNo.ToLower().Contains(q));
            }

            if (departmentId.HasValue && departmentId.Value > 0)
            {
                query = query.Where(s => s.Program != null && s.Program.DepartmentId == departmentId.Value);
            }

            if (programId.HasValue && programId.Value > 0)
            {
                query = query.Where(s => s.ProgramId == programId.Value);
            }

            if (semester.HasValue && semester.Value > 0)
            {
                query = query.Where(s => s.CurrentSemester == semester.Value);
            }

            if (!string.IsNullOrWhiteSpace(section))
            {
                query = query.Where(s => s.Section == section);
            }

            int totalCount = await query.CountAsync();

            var students = await query
                .OrderBy(s => s.ProgramId)
                .ThenBy(s => s.CurrentSemester)
                .ThenBy(s => s.RollNumber)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new StudentListViewModel
            {
                Students = students,
                SearchQuery = searchQuery,
                DepartmentId = departmentId,
                ProgramId = programId,
                Semester = semester,
                Section = section,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Departments = await _context.Departments.AsNoTracking().OrderBy(d => d.Name).ToListAsync(),
                Programs = await _context.AcademicPrograms.AsNoTracking().OrderBy(p => p.Title).ToListAsync()
            };

            return View(viewModel);
        }

        // GET: Admin/Students/EnrollApplicant?applicationId=1
        public async Task<IActionResult> EnrollApplicant(int applicationId)
        {
            var application = await _context.AdmissionApplications
                .Include(a => a.AppliedProgram)
                    .ThenInclude(p => p!.Department)
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (application == null)
            {
                TempData["ErrorMessage"] = "Admission application not found.";
                return RedirectToAction(nameof(Index));
            }

            // Generate sequential Roll Number (e.g. GDCM-BSCS-2026-005)
            var currentYear = DateTime.UtcNow.Year;
            var programCode = application.AppliedProgram?.Department?.Code ?? "GEN";
            var levelPrefix = application.AppliedProgram?.Level == "HSSC" ? "INTER" : programCode;

            // Count existing students in this program and session
            int sequence = await _context.StudentProfiles.CountAsync(s => s.ProgramId == application.AppliedProgramId) + 1;
            var generatedRoll = $"GDCM-{levelPrefix}-{currentYear}-{sequence:D3}";
            var generatedReg = application.AppliedProgram?.Level == "HSSC"
                ? $"BISE-SWAT-{currentYear}-{sequence:D4}"
                : $"UOS-{currentYear}-{levelPrefix}-{sequence:D4}";

            var model = new EnrollApplicantViewModel
            {
                ApplicationId = application.Id,
                ApplicantName = application.ApplicantName,
                FatherName = application.FatherName,
                CNIC = application.CNIC,
                Phone = application.Phone,
                ProgramId = application.AppliedProgramId,
                ProgramTitle = application.AppliedProgram?.Title ?? "Program",
                MeritScore = application.MeritScore,
                RollNumber = generatedRoll,
                RegistrationNo = generatedReg,
                Section = "A",
                CurrentSemester = 1
            };

            return View(model);
        }

        // POST: Admin/Students/EnrollApplicant
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnrollApplicant(EnrollApplicantViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check duplicate roll number
            if (await _context.StudentProfiles.AnyAsync(s => s.RollNumber == model.RollNumber))
            {
                ModelState.AddModelError(nameof(model.RollNumber), "This Roll Number is already assigned to another student.");
                return View(model);
            }

            // Check duplicate CNIC
            if (await _context.StudentProfiles.AnyAsync(s => s.CNIC == model.CNIC))
            {
                ModelState.AddModelError(nameof(model.CNIC), "A student with this CNIC is already enrolled.");
                return View(model);
            }

            var application = await _context.AdmissionApplications
                .Include(a => a.AppliedProgram)
                .FirstOrDefaultAsync(a => a.Id == model.ApplicationId);

            if (application == null)
            {
                TempData["ErrorMessage"] = "Application not found.";
                return RedirectToAction(nameof(Index));
            }

            // 1. Create ASP.NET Core Identity user account
            var cleanCnic = Regex.Replace(model.CNIC, @"[^\d]", "");
            var defaultPassword = cleanCnic.Length >= 6 ? cleanCnic : "Swat@2026";

            var identityUser = new IdentityUser
            {
                UserName = model.RollNumber,
                Email = $"{model.RollNumber.ToLower().Replace('-', '.')}@gdcmadyan.edu.pk",
                PhoneNumber = model.Phone,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(identityUser, defaultPassword);
            if (!createResult.Succeeded)
            {
                foreach (var err in createResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, err.Description);
                }
                return View(model);
            }

            await _userManager.AddToRoleAsync(identityUser, "Student");

            // 2. Create StudentProfile entity
            var student = new StudentProfile
            {
                RollNumber = model.RollNumber,
                RegistrationNo = model.RegistrationNo,
                FullName = model.ApplicantName,
                FatherName = model.FatherName,
                CNIC = model.CNIC,
                DateOfBirth = application.DateOfBirth,
                Phone = model.Phone,
                DomicileDistrict = application.DomicileDistrict,
                Address = application.PostalAddress,
                ProgramId = model.ProgramId,
                CurrentSemester = model.CurrentSemester,
                Section = model.Section,
                CGPA = 0.00m,
                AdmissionDate = DateTime.UtcNow,
                IdentityUserId = identityUser.Id,
                IsActive = true
            };

            _context.StudentProfiles.Add(student);
            await _context.SaveChangesAsync();

            // 3. Update Admission Application Status
            application.Status = "Admitted";

            // 4. Generate Initial Admission & Semester Fee Challan
            int challanSequence = await _context.FeeChallans.CountAsync() + 1001;
            var feeChallan = new FeeChallan
            {
                ChallanNumber = $"BOK-GDCM-{DateTime.UtcNow.Year}-{challanSequence}",
                StudentId = student.Id,
                Semester = model.CurrentSemester,
                TuitionFee = application.AppliedProgram?.FeePerSemester ?? 4500.00m,
                AdmissionFee = 1500.00m,
                ExamFee = 1500.00m,
                LibraryFund = 500.00m,
                LabFund = 500.00m,
                LateFine = 0.00m,
                TotalAmount = (application.AppliedProgram?.FeePerSemester ?? 4500.00m) + 4000.00m,
                IssueDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(15),
                IsPaid = false
            };

            _context.FeeChallans.Add(feeChallan);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Applicant {student.FullName} successfully admitted! Roll Number: {student.RollNumber}. Initial Challan {feeChallan.ChallanNumber} generated.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Students/PromoteBatch
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PromoteBatch(BatchPromotionViewModel model)
        {
            if (model.ProgramId <= 0 || model.CurrentSemester <= 0 || model.NextSemester <= model.CurrentSemester)
            {
                TempData["ErrorMessage"] = "Invalid batch promotion parameters selected.";
                return RedirectToAction(nameof(Index));
            }

            var studentsQuery = _context.StudentProfiles
                .Where(s => s.ProgramId == model.ProgramId && s.CurrentSemester == model.CurrentSemester && s.IsActive);

            if (!string.IsNullOrWhiteSpace(model.Section))
            {
                studentsQuery = studentsQuery.Where(s => s.Section == model.Section);
            }

            var studentsToPromote = await studentsQuery.ToListAsync();

            if (!studentsToPromote.Any())
            {
                TempData["ErrorMessage"] = "No active students found in the specified program and semester.";
                return RedirectToAction(nameof(Index));
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var student in studentsToPromote)
                {
                    student.CurrentSemester = model.NextSemester;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["SuccessMessage"] = $"Successfully promoted {studentsToPromote.Count} students to Semester {model.NextSemester}!";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error promoting batch for program {ProgramId}", model.ProgramId);
                TempData["ErrorMessage"] = "An error occurred during semester batch promotion. Transaction rolled back.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
