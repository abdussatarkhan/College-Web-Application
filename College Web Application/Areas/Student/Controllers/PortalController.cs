using College_Web_Application.Data;
using College_Web_Application.Models;
using College_Web_Application.Models.ViewModels;
using College_Web_Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace College_Web_Application.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student,SuperAdmin,Principal")]
    public class PortalController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IAttendanceCalculatorService _attendanceService;
        private readonly ILogger<PortalController> _logger;

        public PortalController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            IAttendanceCalculatorService attendanceService,
            ILogger<PortalController> logger)
        {
            _context = context;
            _userManager = userManager;
            _attendanceService = attendanceService;
            _logger = logger;
        }

        // Resolves the logged-in student profile or admin preview fallback
        private async Task<StudentProfile?> GetCurrentStudentAsync()
        {
            var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(identityUserId)) return null;

            var student = await _context.StudentProfiles
                .Include(s => s.Program)
                    .ThenInclude(p => p!.Department)
                .FirstOrDefaultAsync(s => s.IdentityUserId == identityUserId && s.IsActive);

            // If user is Admin/Principal previewing the student portal, provide sample student
            if (student == null && (User.IsInRole("SuperAdmin") || User.IsInRole("Principal")))
            {
                student = await _context.StudentProfiles
                    .Include(s => s.Program)
                        .ThenInclude(p => p!.Department)
                    .FirstOrDefaultAsync(s => s.RollNumber == "GDCM-BSCS-26-001" || s.Id == 1);
            }

            return student;
        }

        // GET: /Student/Portal or /Student/Portal/Index
        public async Task<IActionResult> Index()
        {
            var student = await GetCurrentStudentAsync();
            if (student == null)
            {
                _logger.LogWarning("Access Denied: Unlinked or inactive student account accessed Student Portal. User: {User}", User.Identity?.Name);
                return View("StudentNotFound");
            }

            // Fetch live course-by-course attendance evaluations
            var evaluations = await _attendanceService.EvaluateAllCoursesForStudentAsync(student.Id, student.CurrentSemester);

            // Fetch exam results for current semester
            var examResults = await _context.ExamResults
                .Include(e => e.Course)
                .Where(e => e.StudentId == student.Id && e.Semester == student.CurrentSemester)
                .ToListAsync();

            int eligibleCount = evaluations.Count(e => !e.IsDebarredFromFinalExam);
            int warningCount = evaluations.Count(e => e.Status == AttendanceEligibilityStatus.CondonationZone && !e.IsCondonedByPrincipal);
            int debarredCount = evaluations.Count(e => e.Status == AttendanceEligibilityStatus.Debarred && !e.IsCondonedByPrincipal);

            int totalLecturesAttended = evaluations.Sum(e => e.TotalPresent + e.TotalLeave);
            int totalLecturesConducted = evaluations.Sum(e => e.TotalLecturesConducted);
            decimal overallAttendance = totalLecturesConducted > 0 
                ? Math.Round(((decimal)totalLecturesAttended / totalLecturesConducted) * 100m, 1) 
                : 100.0m;

            bool isAnyDebarred = debarredCount > 0 || warningCount > 0;
            bool canDownloadSlip = evaluations.Any(e => !e.IsDebarredFromFinalExam);

            var viewModel = new StudentPortalDashboardViewModel
            {
                Student = student,
                AttendanceEvaluations = evaluations,
                SemesterResults = examResults,
                IsAnyCourseDebarred = isAnyDebarred,
                CanDownloadRollNoSlip = canDownloadSlip,
                EligibleCoursesCount = eligibleCount,
                WarningCoursesCount = warningCount,
                DebarredCoursesCount = debarredCount,
                OverallAttendancePercentage = overallAttendance
            };

            return View(viewModel);
        }

        // GET: /Student/Portal/RollNoSlip
        public async Task<IActionResult> RollNoSlip()
        {
            var student = await GetCurrentStudentAsync();
            if (student == null)
            {
                return NotFound("Student profile record not found.");
            }

            var evaluations = await _attendanceService.EvaluateAllCoursesForStudentAsync(student.Id, student.CurrentSemester);

            var slipCourses = new List<RollNoSlipCourseItem>();
            var baseDate = DateTime.UtcNow.Date.AddDays(14); // 2 weeks out for final term schedule
            int dayOffset = 0;

            foreach (var eval in evaluations)
            {
                bool isEligible = !eval.IsDebarredFromFinalExam;
                string statusText = isEligible ? (eval.IsCondonedByPrincipal ? "CONDONED (ELIGIBLE)" : "ELIGIBLE") : "DEBARRED (SHORT ATTENDANCE)";

                slipCourses.Add(new RollNoSlipCourseItem
                {
                    CourseCode = eval.CourseCode,
                    CourseTitle = eval.CourseTitle,
                    CreditHours = eval.CreditHours,
                    AttendancePercentage = eval.AttendancePercentage,
                    IsEligible = isEligible,
                    IsCondoned = eval.IsCondonedByPrincipal,
                    StatusBadge = statusText,
                    Remarks = isEligible ? "Permitted to appear" : "Ineligible per HEC 75% rule",
                    ExamDate = baseDate.AddDays(dayOffset).ToString("dd-MMM-yyyy (dddd)"),
                    ExamTime = (dayOffset % 2 == 0) ? "09:00 AM - 12:00 PM (Morning)" : "02:00 PM - 05:00 PM (Evening)"
                });

                dayOffset += 2; // Every alternate day
            }

            var viewModel = new StudentRollNoSlipViewModel
            {
                SlipNumber = $"GDCM-RNS-{DateTime.UtcNow.Year}-F-{student.RollNumber.Replace(" ", "-")}",
                Student = student,
                Semester = student.CurrentSemester,
                AcademicSession = "Fall 2026",
                ExamCommencementDate = baseDate,
                ExaminationCenter = "Main Examination Hall, Block-A, GDC Madyan Swat",
                Courses = slipCourses,
                CanAppearInExams = slipCourses.Any(c => c.IsEligible)
            };

            return View(viewModel);
        }
    }
}
