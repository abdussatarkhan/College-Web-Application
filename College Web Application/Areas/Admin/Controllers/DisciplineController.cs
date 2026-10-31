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
    [Authorize(Roles = "Principal,SuperAdmin,Clerk,ControllerExams")]
    public class DisciplineController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DisciplineController> _logger;

        public DisciplineController(
            ApplicationDbContext context,
            ILogger<DisciplineController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Admin/Discipline
        public async Task<IActionResult> Index()
        {
            var records = await _context.RusticationRecords
                .Include(r => r.Student)
                    .ThenInclude(s => s!.Program)
                .Include(r => r.Course)
                .OrderByDescending(r => r.RusticationDate)
                .ToListAsync();

            var viewModels = records.Select(r => new RusticatedStudentItemViewModel
            {
                RecordId = r.Id,
                StudentId = r.StudentId,
                RollNumber = r.Student?.RollNumber ?? "N/A",
                FullName = r.Student?.FullName ?? "N/A",
                FatherName = r.Student?.FatherName ?? "N/A",
                Phone = r.Student?.Phone ?? "03450000000",
                ProgramTitle = r.Student?.Program?.Title ?? "N/A",
                Level = r.Student?.Program?.Level ?? "HSSC",
                Section = r.Student?.Section ?? "A",
                RusticationDate = r.RusticationDate,
                AbsentDatesSummary = r.AbsentDatesSummary,
                Reason = r.Reason,
                CourseTitle = r.Course?.Title ?? "Intermediate Core Subject",
                IsReinstated = r.IsReinstated,
                ParentNameVisited = r.ParentNameVisited,
                ParentCNIC = r.ParentCNIC,
                ParentUndertakingNotes = r.ParentUndertakingNotes,
                ReinstatedDate = r.ReinstatedDate,
                FinePaidAmount = r.FinePaidAmount
            }).ToList();

            return View(viewModels);
        }

        // POST: /Admin/Discipline/ReinstateStudent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReinstateStudent(ReinstateStudentRequestModel model)
        {
            if (string.IsNullOrWhiteSpace(model.ParentNameVisited))
            {
                TempData["ErrorMessage"] = "Parent / Guardian Name is required to document institutional clearance.";
                return RedirectToAction(nameof(Index));
            }

            var record = await _context.RusticationRecords
                .Include(r => r.Student)
                .FirstOrDefaultAsync(r => r.Id == model.RecordId);

            if (record == null)
            {
                TempData["ErrorMessage"] = "Rustication record not found.";
                return RedirectToAction(nameof(Index));
            }

            var student = record.Student;
            if (student == null)
            {
                TempData["ErrorMessage"] = "Associated student profile not found.";
                return RedirectToAction(nameof(Index));
            }

            // Document Parent/Guardian Visit & Undertaking
            record.IsReinstated = true;
            record.ParentNameVisited = model.ParentNameVisited.Trim();
            record.ParentCNIC = string.IsNullOrWhiteSpace(model.ParentCNIC) ? "Not Provided" : model.ParentCNIC.Trim();
            record.ParentUndertakingNotes = string.IsNullOrWhiteSpace(model.ParentUndertakingNotes)
                ? $"Parent {model.ParentNameVisited} visited Principal Office and submitted written undertaking."
                : model.ParentUndertakingNotes.Trim();
            record.FinePaidAmount = Math.Max(model.FinePaidAmount, 0m);
            record.ReinstatedDate = DateTime.UtcNow;
            record.ReinstatedByStaffId = User.Identity?.Name ?? "Principal Office";

            // Un-rusticate student & unlock on teacher attendance sheet
            student.AcademicStatus = AcademicStatus.Active;
            student.ConsecutiveAbsentDays = 0;

            await _context.SaveChangesAsync();

            _logger.LogInformation("DISCIPLINE REINSTATEMENT: Student {Roll} ({Name}) reinstated by {User} following parent visit by {Parent}.",
                student.RollNumber, student.FullName, User.Identity?.Name, model.ParentNameVisited);

            TempData["SuccessMessage"] = $"Student {student.FullName} ({student.RollNumber}) has been officially REINSTATED. Student is now UNLOCKED on the teacher's class attendance register.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Discipline/AttendanceSummary
        public async Task<IActionResult> AttendanceSummary(int? programId, string section = "A")
        {
            var programs = await _context.AcademicPrograms
                .AsNoTracking()
                .OrderBy(p => p.Title)
                .ToListAsync();

            int selectedProgId = programId ?? (programs.FirstOrDefault()?.Id ?? 1);

            var students = await _context.StudentProfiles
                .Include(s => s.Program)
                .Where(s => s.ProgramId == selectedProgId && s.Section == section && s.IsActive)
                .OrderBy(s => s.RollNumber)
                .ToListAsync();

            var rows = new List<StudentAttendanceSummaryRow>();

            foreach (var s in students)
            {
                var records = await _context.AttendanceRecords
                    .Where(a => a.StudentId == s.Id)
                    .ToListAsync();

                int totalHeld = records.Count;
                int presentCount = records.Count(r => r.Status == "Present");
                int absentCount = records.Count(r => r.Status == "Absent");
                int leaveCount = records.Count(r => r.Status == "Leave");

                rows.Add(new StudentAttendanceSummaryRow
                {
                    StudentId = s.Id,
                    RollNumber = s.RollNumber,
                    FullName = s.FullName,
                    FatherName = s.FatherName,
                    ProgramTitle = s.Program?.Title ?? "N/A",
                    TotalClassesHeld = totalHeld,
                    TotalPresent = presentCount,
                    TotalAbsent = absentCount,
                    TotalLeave = leaveCount,
                    ConsecutiveAbsentStreak = s.ConsecutiveAbsentDays,
                    AcademicStatus = s.AcademicStatus
                });
            }

            var viewModel = new AttendanceSummaryFilterViewModel
            {
                SelectedProgramId = selectedProgId,
                SelectedSection = section,
                Programs = programs,
                Rows = rows
            };

            return View(viewModel);
        }
    }
}
