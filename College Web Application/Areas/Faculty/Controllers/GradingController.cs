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
    public class GradingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IAttendanceCalculatorService _attendanceService;
        private readonly ILogger<GradingController> _logger;

        public GradingController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            IAttendanceCalculatorService attendanceService,
            ILogger<GradingController> logger)
        {
            _context = context;
            _userManager = userManager;
            _attendanceService = attendanceService;
            _logger = logger;
        }

        // Resolves the logged-in user to their StaffProfile
        private async Task<StaffProfile?> GetCurrentStaffAsync()
        {
            var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(identityUserId)) return null;

            var staff = await _context.StaffProfiles
                .FirstOrDefaultAsync(s => s.IdentityUserId == identityUserId && s.IsActive);

            // If user is SuperAdmin/Principal without an identity link, fallback to Principal profile
            if (staff == null && (User.IsInRole("SuperAdmin") || User.IsInRole("Principal")))
            {
                staff = await _context.StaffProfiles.FirstOrDefaultAsync(s => s.Id == 1 && s.IsActive);
            }

            return staff;
        }

        // Security Guard: Enforces Subject-Level Security
        private async Task<bool> VerifyCourseOwnershipAsync(int staffId, int courseId, string section, string session)
        {
            // SuperAdmin and Principal have administrative oversight
            if (User.IsInRole("SuperAdmin") || User.IsInRole("Principal"))
            {
                return true;
            }

            return await _context.TeacherCourseAssignments.AnyAsync(a =>
                a.StaffId == staffId &&
                a.CourseId == courseId &&
                a.Section == section &&
                a.AcademicSession == session &&
                a.IsActive);
        }

        // GET: /Faculty/Grading or /Faculty/Grading/MyAssignedCourses
        public async Task<IActionResult> Index()
        {
            var teacher = await GetCurrentStaffAsync();
            if (teacher == null)
            {
                _logger.LogWarning("Access Denied: Unregistered staff attempted to access Grading portal. User: {UserName}", User.Identity?.Name);
                return Forbid();
            }

            IQueryable<TeacherCourseAssignment> query = _context.TeacherCourseAssignments
                .Include(a => a.Course)
                    .ThenInclude(c => c!.Program)
                .Include(a => a.Staff)
                .Where(a => a.IsActive);

            if (!User.IsInRole("SuperAdmin") && !User.IsInRole("Principal"))
            {
                query = query.Where(a => a.StaffId == teacher.Id);
            }

            var assignments = await query.ToListAsync();
            var modelList = new List<AssignedCourseItemViewModel>();

            foreach (var a in assignments)
            {
                if (a.Course == null) continue;

                // Enrolled students matching course program, semester, section
                var enrolledCount = await _context.StudentProfiles
                    .CountAsync(s => s.ProgramId == a.Course.ProgramId &&
                                     s.CurrentSemester == a.Course.Semester &&
                                     s.Section == a.Section &&
                                     s.IsActive);

                var results = await _context.ExamResults
                    .Where(e => e.CourseId == a.CourseId && e.Semester == a.Course.Semester)
                    .ToListAsync();

                var status = results.Any() ? results.First().SubmissionStatus : ExamSubmissionStatus.Draft;
                var remarks = results.FirstOrDefault(r => !string.IsNullOrEmpty(r.ControllerRemarks))?.ControllerRemarks;

                modelList.Add(new AssignedCourseItemViewModel
                {
                    AssignmentId = a.Id,
                    CourseId = a.CourseId,
                    CourseCode = a.Course.CourseCode,
                    CourseTitle = a.Course.Title,
                    CreditHours = a.Course.CreditHours,
                    ProgramTitle = a.Course.Program?.Title ?? "N/A",
                    Level = a.Course.Program?.Level ?? "BS",
                    Semester = a.Course.Semester,
                    Section = a.Section,
                    AcademicSession = a.AcademicSession,
                    EnrolledStudentsCount = enrolledCount,
                    GradedCount = results.Count,
                    Status = status,
                    ControllerRemarks = remarks
                });
            }

            ViewBag.TeacherName = teacher.FullName;
            ViewBag.Designation = teacher.Designation;
            return View(modelList);
        }

        // GET: /Faculty/Grading/EnterMarks
        public async Task<IActionResult> EnterMarks(int? courseId, string section = "A", string session = "Fall 2026")
        {
            var teacher = await GetCurrentStaffAsync();
            if (teacher == null) return Forbid();

            // Default to first assigned course if courseId is not specified
            if (!courseId.HasValue)
            {
                var firstAssignment = await _context.TeacherCourseAssignments
                    .FirstOrDefaultAsync(a => (User.IsInRole("SuperAdmin") || a.StaffId == teacher.Id) && a.IsActive);

                if (firstAssignment == null)
                {
                    TempData["ErrorMessage"] = "No academic subjects are currently assigned to you. Please contact the Head of Department or Principal Office.";
                    return RedirectToAction(nameof(Index));
                }

                courseId = firstAssignment.CourseId;
                section = firstAssignment.Section;
                session = firstAssignment.AcademicSession;
            }

            // Subject Ownership Guard
            bool hasAccess = await VerifyCourseOwnershipAsync(teacher.Id, courseId.Value, section, session);
            if (!hasAccess)
            {
                _logger.LogWarning("SECURITY ALERT: Staff ID {StaffId} ({Name}) attempted unauthorized mark entry for Course ID {CourseId}, Section {Section}.",
                    teacher.Id, teacher.FullName, courseId, section);
                return Forbid();
            }

            var course = await _context.Courses
                .Include(c => c.Program)
                .FirstOrDefaultAsync(c => c.Id == courseId.Value);

            if (course == null) return NotFound("Course not found.");

            // Fetch students enrolled in this program, semester, section
            var students = await _context.StudentProfiles
                .Where(s => s.ProgramId == course.ProgramId &&
                            s.CurrentSemester == course.Semester &&
                            s.Section == section &&
                            s.IsActive)
                .OrderBy(s => s.RollNumber)
                .ToListAsync();

            // Fetch existing marks for this course and semester
            var existingResults = await _context.ExamResults
                .Where(e => e.CourseId == course.Id && e.Semester == course.Semester)
                .ToDictionaryAsync(e => e.StudentId);

            var overallStatus = existingResults.Any()
                ? existingResults.Values.First().SubmissionStatus
                : ExamSubmissionStatus.Draft;

            var remarks = existingResults.Values.FirstOrDefault(r => !string.IsNullOrEmpty(r.ControllerRemarks))?.ControllerRemarks;
            var submittedAt = existingResults.Values.FirstOrDefault(r => r.SubmittedAt.HasValue)?.SubmittedAt;
            var verifiedAt = existingResults.Values.FirstOrDefault(r => r.VerifiedAt.HasValue)?.VerifiedAt;

            var studentMarkItems = new List<StudentMarkEntryItem>();
            bool isBsProgram = course.Program?.Level == "BS";

            foreach (var student in students)
            {
                existingResults.TryGetValue(student.Id, out var res);

                // Live Attendance Compliance Evaluation
                var attEval = await _attendanceService.EvaluateBsCourseAttendanceAsync(student.Id, course.Id);
                bool isDebarred = isBsProgram && attEval.IsDebarredFromFinalExam;

                decimal finalMarks = res?.FinalMarks ?? 0.00m;
                string grade = res?.Grade ?? "F";
                decimal gradePoints = res?.GradePoints ?? 0.00m;

                if (isDebarred)
                {
                    finalMarks = 0.00m;
                    grade = "SA (F)"; // Short Attendance per University of Swat
                    gradePoints = 0.00m;
                }

                studentMarkItems.Add(new StudentMarkEntryItem
                {
                    StudentId = student.Id,
                    RollNumber = student.RollNumber,
                    FullName = student.FullName,
                    FatherName = student.FatherName,
                    AcademicStatus = student.AcademicStatus,
                    MidTermMarks = res?.MidTermMarks ?? 0.00m,
                    SessionalMarks = res?.SessionalMarks ?? 0.00m,
                    FinalMarks = finalMarks,
                    TotalMarks = (res?.MidTermMarks ?? 0.00m) + (res?.SessionalMarks ?? 0.00m) + finalMarks,
                    Grade = grade,
                    GradePoints = gradePoints,
                    AttendancePercentage = attEval.AttendancePercentage,
                    IsDebarredForShortAttendance = isDebarred,
                    IsCondoned = attEval.IsCondonedByPrincipal,
                    AttendanceBadgeClass = attEval.StatusBadgeClass,
                    AttendanceStatusText = attEval.StatusText
                });
            }

            var viewModel = new EnterMarksViewModel
            {
                CourseId = course.Id,
                CourseCode = course.CourseCode,
                CourseTitle = course.Title,
                CreditHours = course.CreditHours,
                ProgramTitle = course.Program?.Title ?? "N/A",
                ProgramLevel = course.Program?.Level ?? "BS",
                Semester = course.Semester,
                Section = section,
                AcademicSession = session,
                TeacherName = teacher.FullName,
                Status = overallStatus,
                ControllerRemarks = remarks,
                SubmittedAt = submittedAt,
                VerifiedAt = verifiedAt,
                StudentMarks = studentMarkItems
            };

            return View(viewModel);
        }

        // POST: /Faculty/Grading/SaveOrSubmitMarks
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveOrSubmitMarks(EnterMarksViewModel model, string actionType)
        {
            var teacher = await GetCurrentStaffAsync();
            if (teacher == null) return Forbid();

            // 1. Strict Subject Ownership Check
            bool hasAccess = await VerifyCourseOwnershipAsync(teacher.Id, model.CourseId, model.Section, model.AcademicSession);
            if (!hasAccess)
            {
                _logger.LogWarning("SECURITY ALERT: Unauthorized POST marks submission by Staff ID {StaffId} for Course {CourseId}.",
                    teacher.Id, model.CourseId);
                return Forbid();
            }

            var course = await _context.Courses
                .Include(c => c.Program)
                .FirstOrDefaultAsync(c => c.Id == model.CourseId);

            if (course == null) return NotFound("Course not found.");

            // 2. Lock Check: If already Submitted to Controller or Approved, disallow modification!
            var currentResults = await _context.ExamResults
                .Where(e => e.CourseId == course.Id && e.Semester == course.Semester)
                .ToListAsync();

            if (currentResults.Any(r => r.SubmissionStatus == ExamSubmissionStatus.SubmittedToController ||
                                        r.SubmissionStatus == ExamSubmissionStatus.ApprovedByController))
            {
                if (!User.IsInRole("SuperAdmin"))
                {
                    TempData["ErrorMessage"] = "This Award List has already been submitted to the Controller of Examinations and is LOCKED against further edits.";
                    return RedirectToAction(nameof(EnterMarks), new { courseId = model.CourseId, section = model.Section, session = model.AcademicSession });
                }
            }

            // Determine target submission status
            bool isFinalSubmission = actionType.Equals("Submit", StringComparison.OrdinalIgnoreCase);
            var targetStatus = isFinalSubmission
                ? ExamSubmissionStatus.SubmittedToController
                : ExamSubmissionStatus.Draft;

            var existingDict = currentResults.ToDictionary(r => r.StudentId);
            var now = DateTime.UtcNow;
            bool isBsCourse = course.Program?.Level == "BS";

            foreach (var item in model.StudentMarks)
            {
                // Validate marks boundary: Mid(0-30), Sessional(0-20), Final(0-50)
                decimal mid = Math.Clamp(item.MidTermMarks, 0m, 30m);
                decimal sessional = Math.Clamp(item.SessionalMarks, 0m, 20m);
                decimal final = Math.Clamp(item.FinalMarks, 0m, 50m);

                // Automatic BS 75% Attendance Compliance Enforcement
                var attEval = await _attendanceService.EvaluateBsCourseAttendanceAsync(item.StudentId, model.CourseId);
                bool isDebarred = isBsCourse && attEval.IsDebarredFromFinalExam;

                decimal total;
                string grade;
                decimal gp;

                if (isDebarred)
                {
                    // Debarred students are forced to FinalMarks = 0 and Grade = SA (Short Attendance)
                    final = 0.00m;
                    total = mid + sessional;
                    grade = "SA (F)";
                    gp = 0.00m;
                }
                else
                {
                    total = mid + sessional + final;
                    var (calculatedGrade, calculatedGp) = CalculateGrade(total);
                    grade = calculatedGrade;
                    gp = calculatedGp;
                }

                if (existingDict.TryGetValue(item.StudentId, out var existing))
                {
                    existing.MidTermMarks = mid;
                    existing.SessionalMarks = sessional;
                    existing.FinalMarks = final;
                    existing.TotalMarks = total;
                    existing.Grade = grade;
                    existing.GradePoints = gp;
                    existing.SubmissionStatus = targetStatus;
                    existing.SubmittedByStaffId = teacher.Id;
                    existing.AttendancePercentageAtExam = attEval.AttendancePercentage;
                    existing.IsDebarredForShortAttendance = isDebarred;

                    if (isFinalSubmission)
                    {
                        existing.SubmittedAt = now;
                        existing.ControllerRemarks = null; // Clear previous correction remarks on fresh submission
                    }
                }
                else
                {
                    var newResult = new ExamResult
                    {
                        StudentId = item.StudentId,
                        CourseId = course.Id,
                        Semester = course.Semester,
                        MidTermMarks = mid,
                        SessionalMarks = sessional,
                        FinalMarks = final,
                        TotalMarks = total,
                        Grade = grade,
                        GradePoints = gp,
                        SubmissionStatus = targetStatus,
                        SubmittedByStaffId = teacher.Id,
                        SubmittedAt = isFinalSubmission ? now : null,
                        AttendancePercentageAtExam = attEval.AttendancePercentage,
                        IsDebarredForShortAttendance = isDebarred
                    };
                    await _context.ExamResults.AddAsync(newResult);
                }
            }

            await _context.SaveChangesAsync();

            if (isFinalSubmission)
            {
                TempData["SuccessMessage"] = $"Award List for {course.CourseCode} - {course.Title} (Section {model.Section}) has been successfully SUBMITTED to the Controller of Examinations. Marks are now LOCKED.";
            }
            else
            {
                TempData["SuccessMessage"] = $"Draft marks for {course.CourseCode} saved successfully. You may continue editing before final submission.";
            }

            return RedirectToAction(nameof(EnterMarks), new { courseId = model.CourseId, section = model.Section, session = model.AcademicSession });
        }

        // Standard HED Khyber Pakhtunkhwa & University of Swat Grading Scale
        private static (string Grade, decimal GradePoints) CalculateGrade(decimal totalMarks)
        {
            if (totalMarks >= 85) return ("A", 4.00m);
            if (totalMarks >= 80) return ("A-", 3.67m);
            if (totalMarks >= 75) return ("B+", 3.33m);
            if (totalMarks >= 70) return ("B", 3.00m);
            if (totalMarks >= 65) return ("B-", 2.67m);
            if (totalMarks >= 60) return ("C+", 2.33m);
            if (totalMarks >= 55) return ("C", 2.00m);
            if (totalMarks >= 50) return ("D", 1.00m);
            return ("F", 0.00m);
        }
    }
}
