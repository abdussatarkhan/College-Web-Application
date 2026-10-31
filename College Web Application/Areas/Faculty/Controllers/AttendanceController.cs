using College_Web_Application.Data;
using College_Web_Application.Models;
using College_Web_Application.Models.ViewModels;
using College_Web_Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace College_Web_Application.Areas.Faculty.Controllers
{
    [Area("Faculty")]
    [Authorize(Roles = "Faculty,HOD,SuperAdmin,Principal")]
    public class AttendanceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IAttendanceComplianceService _complianceService;
        private readonly IParentNotificationService _notificationService;
        private readonly ILogger<AttendanceController> _logger;

        public AttendanceController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            IAttendanceComplianceService complianceService,
            IParentNotificationService notificationService,
            ILogger<AttendanceController> logger)
        {
            _context = context;
            _userManager = userManager;
            _complianceService = complianceService;
            _notificationService = notificationService;
            _logger = logger;
        }

        private async Task<StaffProfile?> GetCurrentStaffAsync()
        {
            var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(identityUserId)) return null;

            var staff = await _context.StaffProfiles
                .FirstOrDefaultAsync(s => s.IdentityUserId == identityUserId && s.IsActive);

            if (staff == null && (User.IsInRole("SuperAdmin") || User.IsInRole("Principal")))
            {
                staff = await _context.StaffProfiles.FirstOrDefaultAsync(s => s.Id == 1 && s.IsActive);
            }

            return staff;
        }

        private async Task<bool> VerifyCourseOwnershipAsync(int staffId, int courseId, string section)
        {
            if (User.IsInRole("SuperAdmin") || User.IsInRole("Principal"))
            {
                return true;
            }

            return await _context.TeacherCourseAssignments.AnyAsync(a =>
                a.StaffId == staffId &&
                a.CourseId == courseId &&
                a.Section == section &&
                a.IsActive);
        }

        // GET: /Faculty/Attendance/MarkClassAttendance
        public async Task<IActionResult> MarkClassAttendance(int? courseId, string section = "A", DateTime? date = null)
        {
            var teacher = await GetCurrentStaffAsync();
            if (teacher == null) return Forbid();

            var targetDate = date ?? DateTime.UtcNow.Date;

            // Fetch assigned courses for this teacher
            IQueryable<TeacherCourseAssignment> assignQuery = _context.TeacherCourseAssignments
                .Include(a => a.Course)
                    .ThenInclude(c => c!.Program)
                .Where(a => a.IsActive);

            if (!User.IsInRole("SuperAdmin") && !User.IsInRole("Principal"))
            {
                assignQuery = assignQuery.Where(a => a.StaffId == teacher.Id);
            }

            var assignedList = await assignQuery.ToListAsync();

            if (!assignedList.Any())
            {
                TempData["ErrorMessage"] = "No academic subjects are currently assigned to your profile. Please contact the Head of Department or Principal Office.";
                return View(new MarkAttendanceViewModel { AssignedCourses = new() });
            }

            // Default to first assigned course if none selected
            int selectedCourseId = courseId ?? assignedList.First().CourseId;
            var currentAssignment = assignedList.FirstOrDefault(a => a.CourseId == selectedCourseId && a.Section == section) 
                                    ?? assignedList.First();

            selectedCourseId = currentAssignment.CourseId;
            section = currentAssignment.Section;

            // Security check
            bool hasAccess = await VerifyCourseOwnershipAsync(teacher.Id, selectedCourseId, section);
            if (!hasAccess)
            {
                _logger.LogWarning("SECURITY ALERT: Staff ID {StaffId} ({Name}) attempted unauthorized attendance access for Course ID {CourseId}, Section {Section}.",
                    teacher.Id, teacher.FullName, selectedCourseId, section);
                return Forbid();
            }

            var course = currentAssignment.Course;
            if (course == null) return NotFound("Course not found.");

            // Fetch students enrolled in this course's program, semester, section
            var enrolledStudents = await _context.StudentProfiles
                .Include(s => s.Program)
                .Where(s => s.ProgramId == course.ProgramId &&
                            s.CurrentSemester == course.Semester &&
                            s.Section == section &&
                            s.IsActive)
                .OrderBy(s => s.RollNumber)
                .ToListAsync();

            // Fetch existing attendance records for targetDate
            var existingRecords = await _context.AttendanceRecords
                .Where(a => a.CourseId == selectedCourseId && a.Date == targetDate)
                .ToDictionaryAsync(a => a.StudentId);

            var studentItems = new List<StudentAttendanceItemViewModel>();
            foreach (var student in enrolledStudents)
            {
                existingRecords.TryGetValue(student.Id, out var rec);
                studentItems.Add(new StudentAttendanceItemViewModel
                {
                    StudentId = student.Id,
                    RollNumber = student.RollNumber,
                    FullName = student.FullName,
                    FatherName = student.FatherName,
                    AcademicStatus = student.AcademicStatus,
                    ConsecutiveAbsentDays = student.ConsecutiveAbsentDays,
                    Status = rec?.Status ?? (student.AcademicStatus == AcademicStatus.Rusticated_ParentRequired ? "Absent" : "Present")
                });
            }

            var optionItems = assignedList.Select(a => new AssignedCourseOptionItem
            {
                CourseId = a.CourseId,
                CourseCode = a.Course?.CourseCode ?? "N/A",
                CourseTitle = a.Course?.Title ?? "N/A",
                Section = a.Section,
                ProgramLevel = a.Course?.Program?.Level ?? "HSSC",
                AcademicSession = a.AcademicSession
            }).ToList();

            var viewModel = new MarkAttendanceViewModel
            {
                CourseId = selectedCourseId,
                CourseCode = course.CourseCode,
                CourseTitle = course.Title,
                ProgramTitle = course.Program?.Title ?? "N/A",
                ProgramLevel = course.Program?.Level ?? "HSSC",
                Semester = course.Semester,
                Section = section,
                AcademicSession = currentAssignment.AcademicSession,
                AttendanceDate = targetDate,
                TeacherName = teacher.FullName,
                Students = studentItems,
                AssignedCourses = optionItems
            };

            return View(viewModel);
        }

        // POST: /Faculty/Attendance/MarkClassAttendance
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkClassAttendance(MarkAttendanceViewModel model)
        {
            var teacher = await GetCurrentStaffAsync();
            if (teacher == null) return Forbid();

            // 1. Strict Subject Ownership Check
            bool hasAccess = await VerifyCourseOwnershipAsync(teacher.Id, model.CourseId, model.Section);
            if (!hasAccess)
            {
                _logger.LogWarning("SECURITY ALERT: Staff ID {StaffId} attempted unauthorized POST attendance for Course {CourseId}.",
                    teacher.Id, model.CourseId);
                return Forbid();
            }

            var course = await _context.Courses
                .Include(c => c.Program)
                .FirstOrDefaultAsync(c => c.Id == model.CourseId);

            if (course == null) return NotFound("Course not found.");

            var targetDate = model.AttendanceDate.Date;
            var statusDict = model.Students.ToDictionary(s => s.StudentId, s => s.Status);

            var evaluationResults = await _complianceService.ProcessBulkClassAttendanceAsync(
                model.CourseId,
                targetDate,
                statusDict,
                teacher.Id);

            var newlyRusticated = evaluationResults.Where(r => r.TriggeredRustication).ToList();
            var bsDebarred = evaluationResults.Where(r => r.TriggeredBsDebarment).ToList();

            if (newlyRusticated.Any())
            {
                var names = string.Join(", ", newlyRusticated.Select(r => $"{r.FullName} ({r.RollNumber})"));
                TempData["RusticationAlert"] = $"🚨 INSTITUTIONAL DISCIPLINE ALERT: {newlyRusticated.Count} HSSC student(s) [{names}] marked absent for 3 consecutive days have been AUTOMATICALLY RUSTICATED (Struck Off). Parent WhatsApp notice has been dispatched. Parent visit to Principal Office is required for clearance.";
            }

            if (bsDebarred.Any())
            {
                var bsNames = string.Join(", ", bsDebarred.Select(r => $"{r.FullName} ({r.BsAttendancePercentage:0.#}%)"));
                TempData["DebarmentAlert"] = $"⚠️ BS 75% ATTENDANCE SHORTAGE: {bsDebarred.Count} student(s) [{bsNames}] have fallen below 75% attendance threshold. Final term exam eligibility locked.";
            }

            TempData["SuccessMessage"] = $"Class attendance for {course.CourseCode} ({model.Section}) on {targetDate:dd MMM yyyy} saved and processed successfully.";
            return RedirectToAction(nameof(MarkClassAttendance), new { courseId = model.CourseId, section = model.Section, date = targetDate.ToString("yyyy-MM-dd") });
        }
    }
}
