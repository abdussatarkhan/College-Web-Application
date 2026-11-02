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

            // Fetch recent notices & announcements
            var notices = await _context.Notices
                .AsNoTracking()
                .OrderByDescending(n => n.PublishedDate)
                .Take(4)
                .ToListAsync();

            // Fetch recent fee challans
            var challans = await _context.FeeChallans
                .AsNoTracking()
                .Where(f => f.StudentId == student.Id)
                .OrderByDescending(f => f.IssueDate)
                .Take(3)
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

            // Compute current SGPA
            decimal totalQp = examResults.Sum(r => (r.Course?.CreditHours ?? 3) * r.GradePoints);
            int totalCr = examResults.Sum(r => r.Course?.CreditHours ?? 3);
            decimal currentSgpa = totalCr > 0 ? Math.Round(totalQp / totalCr, 2) : student.CGPA;

            // Generate weekly schedule
            var weeklySchedule = GetStudentTimetable(student);

            var viewModel = new StudentPortalDashboardViewModel
            {
                Student = student,
                AttendanceEvaluations = evaluations,
                SemesterResults = examResults,
                RecentFeeChallans = challans,
                CollegeAnnouncements = notices,
                WeeklySchedule = weeklySchedule,
                IsAnyCourseDebarred = isAnyDebarred,
                CanDownloadRollNoSlip = canDownloadSlip,
                EligibleCoursesCount = eligibleCount,
                WarningCoursesCount = warningCount,
                DebarredCoursesCount = debarredCount,
                OverallAttendancePercentage = overallAttendance,
                CurrentSgpa = currentSgpa,
                TotalCreditHoursCompleted = student.CurrentSemester * 17
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

            var slipNumber = $"GDCM-RNS-{DateTime.UtcNow.Year}-F-{student.RollNumber.Replace(" ", "-")}";
            var qrUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=140x140&data=https%3A%2F%2Fgdcmadyan.edu.pk%2Fverify%2Frns%2F{Uri.EscapeDataString(slipNumber)}";

            var viewModel = new StudentRollNoSlipViewModel
            {
                SlipNumber = slipNumber,
                Student = student,
                Semester = student.CurrentSemester,
                AcademicSession = "Fall 2026",
                ExamCommencementDate = baseDate,
                ExaminationCenter = "Main Examination Hall, Block-A, GDC Madyan Swat",
                Courses = slipCourses,
                CanAppearInExams = slipCourses.Any(c => c.IsEligible),
                VerificationQrUrl = qrUrl
            };

            return View(viewModel);
        }

        // GET: /Student/Portal/Transcript (Unofficial DMC)
        public async Task<IActionResult> Transcript()
        {
            var student = await GetCurrentStudentAsync();
            if (student == null)
            {
                return NotFound("Student profile record not found.");
            }

            var examResults = await _context.ExamResults
                .Include(e => e.Course)
                .Where(e => e.StudentId == student.Id && e.Semester == student.CurrentSemester)
                .ToListAsync();

            var courseResultItems = new List<DmcCourseResultItem>();
            foreach (var res in examResults)
            {
                int cr = res.Course?.CreditHours ?? 3;
                courseResultItems.Add(new DmcCourseResultItem
                {
                    CourseCode = res.Course?.CourseCode ?? "N/A",
                    CourseTitle = res.Course?.Title ?? "N/A",
                    CreditHours = cr,
                    MidTermMarks = res.MidTermMarks,
                    SessionalMarks = res.SessionalMarks,
                    FinalMarks = res.FinalMarks,
                    TotalMarks = res.TotalMarks,
                    Grade = res.Grade,
                    GradePoints = res.GradePoints,
                    Remarks = res.GradePoints >= 2.00m ? "Pass" : "Fail / Probation"
                });
            }

            decimal totalCr = courseResultItems.Sum(r => r.CreditHours);
            decimal totalQp = courseResultItems.Sum(r => r.QualityPoints);
            decimal sgpa = totalCr > 0 ? Math.Round(totalQp / totalCr, 2) : 0.00m;

            var dmcId = $"GDCM-DMC-{DateTime.UtcNow.Year}-{student.RollNumber}";
            var qrUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=140x140&data=https%3A%2F%2Fgdcmadyan.edu.pk%2Fverify%2Fdmc%2F{Uri.EscapeDataString(dmcId)}";

            var viewModel = new StudentDmcViewModel
            {
                Student = student,
                Semester = student.CurrentSemester,
                AcademicSession = "Fall 2026",
                IssueDate = DateTime.UtcNow,
                CourseResults = courseResultItems,
                TotalCreditHours = totalCr,
                TotalQualityPoints = totalQp,
                Sgpa = sgpa,
                Cgpa = student.CGPA,
                ControllerName = "Dr. Muhammad Tariq Khan (Controller of Examinations)",
                VerificationQrUrl = qrUrl
            };

            return View(viewModel);
        }

        // GET: /Student/Portal/Fees
        public async Task<IActionResult> Fees()
        {
            var student = await GetCurrentStudentAsync();
            if (student == null)
            {
                return NotFound("Student profile record not found.");
            }

            var challans = await _context.FeeChallans
                .AsNoTracking()
                .Where(f => f.StudentId == student.Id)
                .OrderByDescending(f => f.IssueDate)
                .ToListAsync();

            decimal totalBilled = challans.Sum(f => f.TotalAmount);
            decimal totalPaid = challans.Where(f => f.IsPaid).Sum(f => f.TotalAmount);
            decimal totalOutstanding = challans.Where(f => !f.IsPaid).Sum(f => f.TotalAmount);

            var viewModel = new StudentFeeListViewModel
            {
                Student = student,
                Challans = challans,
                TotalBilled = totalBilled,
                TotalPaid = totalPaid,
                TotalOutstanding = totalOutstanding
            };

            return View(viewModel);
        }

        // GET: /Student/Portal/PrintChallan/5 (With Strict IDOR Security)
        public async Task<IActionResult> PrintChallan(int id)
        {
            var student = await GetCurrentStudentAsync();
            if (student == null)
            {
                return Forbid();
            }

            // IDOR Protection: Student can ONLY print their own fee challan
            var challan = await _context.FeeChallans
                .AsNoTracking()
                .Include(f => f.Student)
                    .ThenInclude(s => s!.Program)
                        .ThenInclude(p => p!.Department)
                .FirstOrDefaultAsync(f => f.Id == id && f.StudentId == student.Id);

            if (challan == null)
            {
                _logger.LogWarning("SECURITY ALERT: Student {Roll} attempted unauthorized access to Challan ID {Id}.",
                    student.RollNumber, id);
                return NotFound("Fee challan record not found or does not belong to your account.");
            }

            return View(challan);
        }

        // GET: /Student/Portal/Timetable
        public async Task<IActionResult> Timetable()
        {
            var student = await GetCurrentStudentAsync();
            if (student == null)
            {
                return NotFound("Student profile record not found.");
            }

            var schedule = GetStudentTimetable(student);
            ViewBag.Student = student;
            return View(schedule);
        }

        private List<StudentTimetableItem> GetStudentTimetable(StudentProfile student)
        {
            return new List<StudentTimetableItem>
            {
                new() { DayOfWeek = "Monday", CourseCode = "CS-301", CourseTitle = "Database Systems & Design", TimeSlot = "08:30 AM - 10:00 AM", Room = "Software Lab-1 (Block-A)", Instructor = "Mr. Irfan Ullah" },
                new() { DayOfWeek = "Monday", CourseCode = "CS-302", CourseTitle = "Operating Systems Principles", TimeSlot = "10:15 AM - 11:45 AM", Room = "Lecture Hall-4 (Block-B)", Instructor = "Prof. Dr. Muhammad Tariq Khan" },
                new() { DayOfWeek = "Tuesday", CourseCode = "CS-303", CourseTitle = "Web Technologies & ASP.NET Core", TimeSlot = "08:30 AM - 10:00 AM", Room = "Software Lab-2 (Block-A)", Instructor = "Mr. Irfan Ullah" },
                new() { DayOfWeek = "Tuesday", CourseCode = "MATH-301", CourseTitle = "Linear Algebra & Probability", TimeSlot = "10:15 AM - 11:45 AM", Room = "Lecture Hall-2 (Block-B)", Instructor = "Prof. Noor Muhammad" },
                new() { DayOfWeek = "Wednesday", CourseCode = "CS-301", CourseTitle = "Database Systems (Practical Lab)", TimeSlot = "08:30 AM - 11:30 AM", Room = "Software Lab-1 (Block-A)", Instructor = "Mr. Irfan Ullah" },
                new() { DayOfWeek = "Thursday", CourseCode = "CS-302", CourseTitle = "Operating Systems Principles", TimeSlot = "09:00 AM - 10:30 AM", Room = "Lecture Hall-4 (Block-B)", Instructor = "Prof. Dr. Muhammad Tariq Khan" },
                new() { DayOfWeek = "Thursday", CourseCode = "CS-303", CourseTitle = "Web Technologies & Cloud", TimeSlot = "11:00 AM - 12:30 PM", Room = "Software Lab-2 (Block-A)", Instructor = "Mr. Irfan Ullah" },
                new() { DayOfWeek = "Friday", CourseCode = "ISL-101", CourseTitle = "Islamic Studies & Professional Ethics", TimeSlot = "09:00 AM - 10:30 AM", Room = "Main College Auditorium", Instructor = "Prof. Maulana Abdul Haq" }
            };
        }
    }
}
