using College_Web_Application.Data;
using College_Web_Application.Models;
using Microsoft.EntityFrameworkCore;

namespace College_Web_Application.Services
{
    public enum AttendanceEligibilityStatus
    {
        Eligible = 0,           // >= 75.0%
        CondonationZone = 1,    // 65.0% to 74.9%
        Debarred = 2,           // < 65.0%
        NotEnoughData = 3       // < 10 lectures held yet (default safe)
    }

    public class CourseAttendanceEvaluation
    {
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public int CreditHours { get; set; } = 3;
        public int TotalLecturesConducted { get; set; }
        public int TotalPresent { get; set; }
        public int TotalLeave { get; set; }
        public int TotalAbsent { get; set; }
        public decimal AttendancePercentage { get; set; } = 100.00m;
        public AttendanceEligibilityStatus Status { get; set; } = AttendanceEligibilityStatus.Eligible;
        public bool IsCondonedByPrincipal { get; set; } = false;
        public bool IsDebarredFromFinalExam { get; set; } = false;
        public int ClassesNeededToReach75 { get; set; } = 0;
        public string StatusBadgeClass { get; set; } = "bg-success";
        public string StatusText { get; set; } = "Eligible (75%+)";
        public string RecommendationMessage { get; set; } = string.Empty;
    }

    public interface IAttendanceCalculatorService
    {
        Task<CourseAttendanceEvaluation> EvaluateBsCourseAttendanceAsync(int studentId, int courseId);
        Task<List<CourseAttendanceEvaluation>> EvaluateAllCoursesForStudentAsync(int studentId, int semester);
        Task<Dictionary<int, CourseAttendanceEvaluation>> EvaluateClassAttendanceForCourseAsync(int courseId, string section);
        Task<(bool ShouldRusticate, List<DateTime> AbsentDates)> CheckHsscConsecutiveAbsenceAsync(int studentId, int courseId, DateTime targetDate);
    }

    public class AttendanceCalculatorService : IAttendanceCalculatorService
    {
        private readonly ApplicationDbContext _context;

        public AttendanceCalculatorService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CourseAttendanceEvaluation> EvaluateBsCourseAttendanceAsync(int studentId, int courseId)
        {
            var course = await _context.Courses
                .AsNoTracking()
                .Include(c => c.Program)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            var student = await _context.StudentProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == studentId);

            var records = await _context.AttendanceRecords
                .AsNoTracking()
                .Where(a => a.StudentId == studentId && a.CourseId == courseId)
                .ToListAsync();

            // Check if there is an approved condonation from the Principal for this student and course
            var condonation = await _context.AttendanceCondonations
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.StudentId == studentId && c.CourseId == courseId && c.IsApprovedByPrincipal);

            bool isCondoned = condonation != null;

            int totalConducted = records.Count;
            int present = records.Count(r => r.Status == "Present");
            int leave = records.Count(r => r.Status == "Leave");
            int absent = records.Count(r => r.Status == "Absent");

            // Approved official duty/medical leaves count toward attendance per Swat University/HEC policy
            int effectiveAttended = present + leave;

            decimal percentage = 100.00m;
            if (totalConducted > 0)
            {
                percentage = Math.Round(((decimal)effectiveAttended / totalConducted) * 100m, 2);
            }

            // Calculate classes needed to reach 75%
            // Formula: x = 3C - 4P
            int classesNeeded = 0;
            if (percentage < 75.0m && totalConducted > 0)
            {
                int diff = (3 * totalConducted) - (4 * effectiveAttended);
                classesNeeded = Math.Max(0, diff);
            }

            // Determine status
            AttendanceEligibilityStatus status;
            bool isDebarred = false;
            string badgeClass;
            string statusText;
            string recommendation;

            if (totalConducted < 10)
            {
                // Early in semester: minimum 10 lectures held to trigger debarment
                status = AttendanceEligibilityStatus.NotEnoughData;
                badgeClass = "bg-info text-dark";
                statusText = $"{percentage:0.#}% (Initial Sessions)";
                recommendation = $"Safe: {totalConducted} class(es) held so far. Keep attendance above 75%.";
            }
            else if (percentage >= 75.0m)
            {
                status = AttendanceEligibilityStatus.Eligible;
                badgeClass = "bg-success text-white";
                statusText = $"Eligible ({percentage:0.#}%)";
                recommendation = $"Safe! You have attended {effectiveAttended} of {totalConducted} classes ({percentage:0.#}%). Final Exam Roll No Slip unlocked.";
            }
            else if (percentage >= 65.0m)
            {
                status = AttendanceEligibilityStatus.CondonationZone;
                if (isCondoned)
                {
                    badgeClass = "bg-success text-white";
                    statusText = $"Condoned ({percentage:0.#}%)";
                    recommendation = $"Medical Condonation approved by Principal Office. Eligible for Final Exam.";
                    isDebarred = false;
                }
                else
                {
                    badgeClass = "bg-warning text-dark";
                    statusText = $"Warning ({percentage:0.#}%)";
                    recommendation = $"Warning: Your attendance in {course?.CourseCode ?? "this course"} is {percentage:0.#}%. You must attend the next {classesNeeded} class(es) consecutively to reach 75%, or apply for Principal Condonation.";
                    isDebarred = true;
                }
            }
            else
            {
                status = AttendanceEligibilityStatus.Debarred;
                if (isCondoned)
                {
                    badgeClass = "bg-warning text-dark";
                    statusText = $"Condoned Waiver ({percentage:0.#}%)";
                    recommendation = "Special waiver granted by Principal/Academic Council.";
                    isDebarred = false;
                }
                else
                {
                    status = AttendanceEligibilityStatus.Debarred;
                    badgeClass = "bg-danger text-white";
                    statusText = $"DEBARRED ({percentage:0.#}%)";
                    recommendation = $"Strictly Debarred: Attendance is {percentage:0.#}% (<65%). Ineligible for Final Term Exam per HEC & University of Swat regulations.";
                    isDebarred = true;
                }
            }

            return new CourseAttendanceEvaluation
            {
                StudentId = studentId,
                CourseId = courseId,
                CourseCode = course?.CourseCode ?? "N/A",
                CourseTitle = course?.Title ?? "N/A",
                CreditHours = course?.CreditHours ?? 3,
                TotalLecturesConducted = totalConducted,
                TotalPresent = present,
                TotalLeave = leave,
                TotalAbsent = absent,
                AttendancePercentage = percentage,
                Status = status,
                IsCondonedByPrincipal = isCondoned,
                IsDebarredFromFinalExam = isDebarred,
                ClassesNeededToReach75 = classesNeeded,
                StatusBadgeClass = badgeClass,
                StatusText = statusText,
                RecommendationMessage = recommendation
            };
        }

        public async Task<List<CourseAttendanceEvaluation>> EvaluateAllCoursesForStudentAsync(int studentId, int semester)
        {
            var student = await _context.StudentProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null) return new List<CourseAttendanceEvaluation>();

            var courses = await _context.Courses
                .AsNoTracking()
                .Where(c => c.ProgramId == student.ProgramId && c.Semester == semester)
                .ToListAsync();

            var evaluations = new List<CourseAttendanceEvaluation>();
            foreach (var c in courses)
            {
                evaluations.Add(await EvaluateBsCourseAttendanceAsync(studentId, c.Id));
            }

            return evaluations;
        }

        public async Task<Dictionary<int, CourseAttendanceEvaluation>> EvaluateClassAttendanceForCourseAsync(int courseId, string section)
        {
            var course = await _context.Courses
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null) return new Dictionary<int, CourseAttendanceEvaluation>();

            var studentIds = await _context.StudentProfiles
                .AsNoTracking()
                .Where(s => s.ProgramId == course.ProgramId && s.CurrentSemester == course.Semester && s.Section == section && s.IsActive)
                .Select(s => s.Id)
                .ToListAsync();

            var dict = new Dictionary<int, CourseAttendanceEvaluation>();
            foreach (var sid in studentIds)
            {
                dict[sid] = await EvaluateBsCourseAttendanceAsync(sid, courseId);
            }

            return dict;
        }

        public async Task<(bool ShouldRusticate, List<DateTime> AbsentDates)> CheckHsscConsecutiveAbsenceAsync(int studentId, int courseId, DateTime targetDate)
        {
            var pastRecords = await _context.AttendanceRecords
                .AsNoTracking()
                .Where(a => a.StudentId == studentId && a.CourseId == courseId && a.Date < targetDate)
                .OrderByDescending(a => a.Date)
                .Take(2)
                .ToListAsync();

            if (pastRecords.Count == 2 && pastRecords[0].Status == "Absent" && pastRecords[1].Status == "Absent")
            {
                var dates = new List<DateTime> { pastRecords[1].Date, pastRecords[0].Date, targetDate };
                return (true, dates);
            }

            return (false, new List<DateTime>());
        }
    }
}
