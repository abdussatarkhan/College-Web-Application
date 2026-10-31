using College_Web_Application.Data;
using College_Web_Application.Models;
using College_Web_Application.Models.ViewModels;
using College_Web_Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace College_Web_Application.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ControllerExams,Principal,SuperAdmin")]
    public class ExamControllerOfficeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IAttendanceCalculatorService _attendanceService;
        private readonly ILogger<ExamControllerOfficeController> _logger;

        public ExamControllerOfficeController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            IAttendanceCalculatorService attendanceService,
            ILogger<ExamControllerOfficeController> logger)
        {
            _context = context;
            _userManager = userManager;
            _attendanceService = attendanceService;
            _logger = logger;
        }

        // GET: /Admin/ExamControllerOffice
        public async Task<IActionResult> Index(int? programId, int? semester)
        {
            var programs = await _context.AcademicPrograms
                .AsNoTracking()
                .OrderBy(p => p.Title)
                .ToListAsync();

            int selectedProgId = programId ?? (programs.FirstOrDefault()?.Id ?? 4);
            int selectedSem = semester ?? 1;

            // Fetch all courses in this Program and Semester
            var courses = await _context.Courses
                .AsNoTracking()
                .Where(c => c.ProgramId == selectedProgId && c.Semester == selectedSem)
                .ToListAsync();

            var courseStatuses = new List<CourseAwardListStatusItem>();

            foreach (var course in courses)
            {
                // Find assigned teacher
                var assignment = await _context.TeacherCourseAssignments
                    .Include(a => a.Staff)
                    .FirstOrDefaultAsync(a => a.CourseId == course.Id && a.IsActive);

                var results = await _context.ExamResults
                    .Where(e => e.CourseId == course.Id && e.Semester == selectedSem)
                    .ToListAsync();

                var enrolledCount = await _context.StudentProfiles
                    .CountAsync(s => s.ProgramId == selectedProgId && s.CurrentSemester == selectedSem && s.IsActive);

                var status = results.Any() ? results.First().SubmissionStatus : ExamSubmissionStatus.Draft;
                var remarks = results.FirstOrDefault(r => !string.IsNullOrEmpty(r.ControllerRemarks))?.ControllerRemarks;
                var submittedAt = results.FirstOrDefault(r => r.SubmittedAt.HasValue)?.SubmittedAt;
                var verifiedAt = results.FirstOrDefault(r => r.VerifiedAt.HasValue)?.VerifiedAt;

                courseStatuses.Add(new CourseAwardListStatusItem
                {
                    CourseId = course.Id,
                    CourseCode = course.CourseCode,
                    Title = course.Title,
                    CreditHours = course.CreditHours,
                    TeacherName = assignment?.Staff?.FullName ?? "Unassigned",
                    Section = assignment?.Section ?? "A",
                    AcademicSession = assignment?.AcademicSession ?? "Fall 2026",
                    TotalStudents = enrolledCount,
                    GradedStudents = results.Count,
                    Status = status,
                    SubmittedAt = submittedAt,
                    VerifiedAt = verifiedAt,
                    ControllerRemarks = remarks
                });
            }

            // Check if 100% of courses in this semester are approved
            bool allApproved = courseStatuses.Any() && courseStatuses.All(c => c.Status == ExamSubmissionStatus.ApprovedByController);

            // Fetch students in this semester for Master DMC generator list
            var students = await _context.StudentProfiles
                .AsNoTracking()
                .Where(s => s.ProgramId == selectedProgId && s.CurrentSemester == selectedSem && s.IsActive)
                .OrderBy(s => s.RollNumber)
                .ToListAsync();

            var studentDmcSummaries = new List<StudentSummaryDmcItem>();
            foreach (var s in students)
            {
                var approvedResults = await _context.ExamResults
                    .Include(e => e.Course)
                    .Where(e => e.StudentId == s.Id && e.Semester == selectedSem && e.SubmissionStatus == ExamSubmissionStatus.ApprovedByController)
                    .ToListAsync();

                decimal totalQp = approvedResults.Sum(r => (r.Course?.CreditHours ?? 3) * r.GradePoints);
                int totalCr = approvedResults.Sum(r => r.Course?.CreditHours ?? 3);
                decimal sgpa = totalCr > 0 ? Math.Round(totalQp / totalCr, 2) : 0.00m;

                studentDmcSummaries.Add(new StudentSummaryDmcItem
                {
                    StudentId = s.Id,
                    RollNumber = s.RollNumber,
                    FullName = s.FullName,
                    FatherName = s.FatherName,
                    CompletedCoursesCount = approvedResults.Count,
                    TotalSemesterCoursesCount = courses.Count,
                    SGPA = sgpa,
                    CGPA = s.CGPA
                });
            }

            var viewModel = new ControllerDashboardViewModel
            {
                SelectedProgramId = selectedProgId,
                SelectedSemester = selectedSem,
                Programs = programs,
                CourseStatuses = courseStatuses,
                StudentsInSemester = studentDmcSummaries,
                AllCoursesApproved = allApproved
            };

            return View(viewModel);
        }

        // POST: /Admin/ExamControllerOffice/ReviewAwardList
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReviewAwardList(int courseId, int semester, string decision, string? remarks, int programId)
        {
            var results = await _context.ExamResults
                .Where(e => e.CourseId == courseId && e.Semester == semester)
                .ToListAsync();

            if (!results.Any())
            {
                TempData["ErrorMessage"] = "No marks have been recorded or submitted by the teacher for this course.";
                return RedirectToAction(nameof(Index), new { programId, semester });
            }

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
            string controllerId = User.Identity?.Name ?? "Controller Office";
            var now = DateTime.UtcNow;

            if (decision == "Approve")
            {
                foreach (var r in results)
                {
                    r.SubmissionStatus = ExamSubmissionStatus.ApprovedByController;
                    r.VerifiedByControllerId = controllerId;
                    r.VerifiedAt = now;
                    r.ControllerRemarks = null;
                }

                await _context.SaveChangesAsync();

                // Recalculate CGPA for affected students
                var studentIds = results.Select(r => r.StudentId).Distinct().ToList();
                foreach (var sid in studentIds)
                {
                    await RecalculateStudentCgpaAsync(sid);
                }

                TempData["SuccessMessage"] = $"Award List for {course?.CourseCode} ({course?.Title}) successfully VERIFIED and APPROVED. Marks are permanently locked into the Master Gazette.";
            }
            else if (decision == "Return")
            {
                foreach (var r in results)
                {
                    r.SubmissionStatus = ExamSubmissionStatus.ReturnedForCorrection;
                    r.ControllerRemarks = string.IsNullOrWhiteSpace(remarks)
                        ? "Returned for correction by Controller of Examinations. Please recheck mid/sessional marks."
                        : remarks;
                }

                await _context.SaveChangesAsync();
                TempData["WarningMessage"] = $"Award List for {course?.CourseCode} RETURNED to the subject instructor for correction with your remarks.";
            }

            return RedirectToAction(nameof(Index), new { programId, semester });
        }

        // GET: /Admin/ExamControllerOffice/GenerateSemesterDMC
        public async Task<IActionResult> GenerateSemesterDMC(int studentId, int semester)
        {
            var student = await _context.StudentProfiles
                .Include(s => s.Program)
                    .ThenInclude(p => p!.Department)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null) return NotFound("Student profile not found.");

            // Fetch all courses prescribed for this semester
            var prescribedCourses = await _context.Courses
                .Where(c => c.ProgramId == student.ProgramId && c.Semester == semester)
                .ToListAsync();

            // Fetch student results for this semester
            var results = await _context.ExamResults
                .Include(e => e.Course)
                .Where(e => e.StudentId == student.Id && e.Semester == semester)
                .ToListAsync();

            var unverified = new List<string>();
            var courseGradeItems = new List<DmcCourseGradeItem>();

            foreach (var pc in prescribedCourses)
            {
                var res = results.FirstOrDefault(r => r.CourseId == pc.Id);
                var status = res?.SubmissionStatus ?? ExamSubmissionStatus.Draft;

                if (status != ExamSubmissionStatus.ApprovedByController)
                {
                    unverified.Add($"{pc.CourseCode} - {pc.Title} (Status: {status})");
                }

                courseGradeItems.Add(new DmcCourseGradeItem
                {
                    CourseCode = pc.CourseCode,
                    CourseTitle = pc.Title,
                    CreditHours = pc.CreditHours,
                    MidTermMarks = res?.MidTermMarks ?? 0,
                    SessionalMarks = res?.SessionalMarks ?? 0,
                    FinalMarks = res?.FinalMarks ?? 0,
                    TotalMarks = res?.TotalMarks ?? 0,
                    Grade = res?.Grade ?? "F",
                    GradePoints = res?.GradePoints ?? 0,
                    Status = status
                });
            }

            bool fullyVerified = unverified.Count == 0 && prescribedCourses.Any();

            int totalCr = courseGradeItems.Sum(c => c.CreditHours);
            decimal totalQp = courseGradeItems.Sum(c => c.QualityPoints);
            decimal sgpa = totalCr > 0 ? Math.Round(totalQp / totalCr, 2) : 0.00m;

            var dmcModel = new OfficialDmcViewModel
            {
                SerialNumber = $"GDCM-DMC-{(student.Program?.Level == "HSSC" ? "BISE" : "UOS")}-{DateTime.UtcNow.Year}-{student.Id:D4}",
                StudentId = student.Id,
                RollNumber = student.RollNumber,
                RegistrationNo = student.RegistrationNo,
                FullName = student.FullName,
                FatherName = student.FatherName,
                ProgramTitle = student.Program?.Title ?? "N/A",
                DepartmentName = student.Program?.Department?.Name ?? "Higher Education Dept",
                Semester = semester,
                AcademicSession = $"Session {DateTime.UtcNow.Year - 1}-{DateTime.UtcNow.Year}",
                IssueDate = DateTime.UtcNow,
                IsFullyVerified = fullyVerified,
                UnverifiedCourseCodes = unverified,
                Courses = courseGradeItems,
                TotalCreditHours = totalCr,
                TotalQualityPoints = totalQp,
                SGPA = sgpa,
                CGPA = student.CGPA > 0 ? student.CGPA : sgpa,
                Remarks = sgpa >= 2.00m ? "PROMOTED (PASSED)" : "PROBATION",
                QRVerificationPayload = $"https://gdcmadyan.edu.pk/verify/dmc?roll={student.RollNumber}&sem={semester}&sn=GDCM-{student.Id}"
            };

            return View(dmcModel);
        }

        // GET: /Admin/ExamControllerOffice/ShortageList
        public async Task<IActionResult> ShortageList(int? programId, int? semester)
        {
            var bsPrograms = await _context.AcademicPrograms
                .AsNoTracking()
                .Where(p => p.Level == "BS")
                .OrderBy(p => p.Title)
                .ToListAsync();

            int selectedProgId = programId ?? (bsPrograms.FirstOrDefault()?.Id ?? 4);
            int selectedSem = semester ?? 5; // default to semester with seeded data, or 1

            var currentProgram = bsPrograms.FirstOrDefault(p => p.Id == selectedProgId)
                ?? await _context.AcademicPrograms.FindAsync(selectedProgId);

            // Fetch courses in this Program and Semester
            var courses = await _context.Courses
                .AsNoTracking()
                .Where(c => c.ProgramId == selectedProgId && c.Semester == selectedSem)
                .ToListAsync();

            // Fetch enrolled active students
            var students = await _context.StudentProfiles
                .AsNoTracking()
                .Where(s => s.ProgramId == selectedProgId && s.CurrentSemester == selectedSem && s.IsActive)
                .OrderBy(s => s.RollNumber)
                .ToListAsync();

            var shortageItems = new List<AttendanceShortageItemViewModel>();
            int totalEvaluated = 0;
            int totalEligible = 0;
            int totalWarning = 0;
            int totalDebarred = 0;

            foreach (var st in students)
            {
                foreach (var course in courses)
                {
                    totalEvaluated++;
                    var eval = await _attendanceService.EvaluateBsCourseAttendanceAsync(st.Id, course.Id);

                    if (eval.Status == AttendanceEligibilityStatus.Eligible || eval.Status == AttendanceEligibilityStatus.NotEnoughData)
                    {
                        totalEligible++;
                    }
                    else if (eval.Status == AttendanceEligibilityStatus.CondonationZone)
                    {
                        totalWarning++;
                    }
                    else if (eval.Status == AttendanceEligibilityStatus.Debarred)
                    {
                        totalDebarred++;
                    }

                    // If not eligible (or condoned but has shortage history), add to shortage notice
                    if (eval.AttendancePercentage < 75.0m && eval.TotalLecturesConducted >= 1)
                    {
                        var condonation = await _context.AttendanceCondonations
                            .AsNoTracking()
                            .FirstOrDefaultAsync(c => c.StudentId == st.Id && c.CourseId == course.Id);

                        string cleanPhone = new string((st.Phone ?? "923000000000").Where(char.IsDigit).ToArray());
                        if (!cleanPhone.StartsWith("92") && cleanPhone.StartsWith("0"))
                        {
                            cleanPhone = "92" + cleanPhone.Substring(1);
                        }
                        else if (!cleanPhone.StartsWith("92"))
                        {
                            cleanPhone = "92" + cleanPhone;
                        }

                        string msg = $"*GOVERNMENT DEGREE COLLEGE MADYAN SWAT*\n" +
                                     $"*OFFICE OF CONTROLLER OF EXAMINATIONS*\n\n" +
                                     $"*URGENT ATTENDANCE SHORTAGE NOTICE*\n" +
                                     $"Student: {st.FullName} (Roll No: {st.RollNumber})\n" +
                                     $"Course: {course.CourseCode} - {course.Title}\n" +
                                     $"Current Attendance: {eval.AttendancePercentage:0.#}%\n" +
                                     $"Lectures Conducted: {eval.TotalLecturesConducted} | Attended: {eval.TotalPresent + eval.TotalLeave}\n" +
                                     $"Status: {(eval.IsCondonedByPrincipal ? "Condoned Waiver Active" : (eval.Status == AttendanceEligibilityStatus.CondonationZone ? "WARNING (Condonation Zone 65-74%)" : "STRICTLY DEBARRED (<65%)"))}\n\n" +
                                     (eval.ClassesNeededToReach75 > 0 ? $"Classes needed to reach 75%: {eval.ClassesNeededToReach75} consecutive lectures.\n" : "") +
                                     $"Per HEC / University of Swat regulations, 75% attendance is mandatory to appear in Final Term Examination. Please contact the Principal Office immediately.";

                        string whatsappUrl = $"https://wa.me/{cleanPhone}?text={Uri.EscapeDataString(msg)}";

                        shortageItems.Add(new AttendanceShortageItemViewModel
                        {
                            StudentId = st.Id,
                            RollNumber = st.RollNumber,
                            FullName = st.FullName,
                            FatherName = st.FatherName,
                            ContactNumber = st.Phone ?? "N/A",
                            CourseId = course.Id,
                            CourseCode = course.CourseCode,
                            CourseTitle = course.Title,
                            Semester = selectedSem,
                            TotalConducted = eval.TotalLecturesConducted,
                            TotalPresent = eval.TotalPresent,
                            TotalLeave = eval.TotalLeave,
                            TotalAbsent = eval.TotalAbsent,
                            AttendancePercentage = eval.AttendancePercentage,
                            Status = eval.Status,
                            ClassesNeededToReach75 = eval.ClassesNeededToReach75,
                            IsCondonedByPrincipal = eval.IsCondonedByPrincipal,
                            CondonationChallan = condonation?.FineChallanNumber,
                            CondonationReason = condonation?.ReasonAndMedicalProof,
                            WhatsAppUrl = whatsappUrl
                        });
                    }
                }
            }

            var model = new AttendanceShortageListViewModel
            {
                SelectedProgramId = selectedProgId,
                SelectedSemester = selectedSem,
                ProgramTitle = currentProgram?.Title ?? "BS Programs",
                Programs = bsPrograms,
                ShortageStudents = shortageItems,
                TotalEvaluated = totalEvaluated,
                TotalEligible = totalEligible,
                TotalWarningCondonationZone = totalWarning,
                TotalDebarred = totalDebarred
            };

            return View(model);
        }

        // POST: /Admin/ExamControllerOffice/ApproveCondonation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveCondonation(int studentId, int courseId, int semester, string reason, string challanNo, int programId)
        {
            var student = await _context.StudentProfiles.FindAsync(studentId);
            var course = await _context.Courses.FindAsync(courseId);

            if (student == null || course == null)
            {
                TempData["ErrorMessage"] = "Student or Course record not found.";
                return RedirectToAction(nameof(ShortageList), new { programId, semester });
            }

            var eval = await _attendanceService.EvaluateBsCourseAttendanceAsync(studentId, courseId);

            var existingCondonation = await _context.AttendanceCondonations
                .FirstOrDefaultAsync(c => c.StudentId == studentId && c.CourseId == courseId);

            string principalId = User.Identity?.Name ?? "Principal Office";

            if (existingCondonation != null)
            {
                existingCondonation.IsApprovedByPrincipal = true;
                existingCondonation.ReasonAndMedicalProof = reason;
                existingCondonation.FineChallanNumber = challanNo;
                existingCondonation.ApprovedByUserId = principalId;
                existingCondonation.ApprovedAt = DateTime.UtcNow;
                existingCondonation.ActualAttendancePercentage = eval.AttendancePercentage;
            }
            else
            {
                var condonation = new AttendanceCondonation
                {
                    StudentId = studentId,
                    CourseId = courseId,
                    Semester = semester,
                    ActualAttendancePercentage = eval.AttendancePercentage,
                    ReasonAndMedicalProof = reason,
                    FineChallanNumber = challanNo,
                    IsApprovedByPrincipal = true,
                    ApprovedByUserId = principalId,
                    ApprovedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.AttendanceCondonations.AddAsync(condonation);
            }

            // Update any existing ExamResult record for this student and course
            var examResult = await _context.ExamResults
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId && e.Semester == semester);
            if (examResult != null)
            {
                examResult.IsDebarredForShortAttendance = false;
                examResult.AttendancePercentageAtExam = eval.AttendancePercentage;
                if (examResult.Grade == "SA (F)")
                {
                    examResult.Grade = "F";
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Attendance Condonation Waiver APPROVED for {student.FullName} ({student.RollNumber}) in {course.CourseCode} with Challan #{challanNo}. Final term exam eligibility restored.";
            return RedirectToAction(nameof(ShortageList), new { programId, semester });
        }

        private async Task RecalculateStudentCgpaAsync(int studentId)
        {
            var approvedResults = await _context.ExamResults
                .Include(e => e.Course)
                .Where(e => e.StudentId == studentId && e.SubmissionStatus == ExamSubmissionStatus.ApprovedByController)
                .ToListAsync();

            if (!approvedResults.Any()) return;

            decimal totalQualityPoints = approvedResults.Sum(r => (r.Course?.CreditHours ?? 3) * r.GradePoints);
            int totalCreditHours = approvedResults.Sum(r => r.Course?.CreditHours ?? 3);

            if (totalCreditHours > 0)
            {
                var student = await _context.StudentProfiles.FindAsync(studentId);
                if (student != null)
                {
                    student.CGPA = Math.Round(totalQualityPoints / totalCreditHours, 2);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}
