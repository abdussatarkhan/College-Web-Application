using College_Web_Application.Data;
using College_Web_Application.Models;
using Microsoft.EntityFrameworkCore;

namespace College_Web_Application.Services
{
    public class AttendanceEvaluationResult
    {
        public int StudentId { get; set; }
        public string RollNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string FatherName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string ProgramLevel { get; set; } = "HSSC";
        public bool TriggeredRustication { get; set; }
        public string? RusticationDatesSummary { get; set; }
        public bool TriggeredBsDebarment { get; set; }
        public decimal BsAttendancePercentage { get; set; } = 100m;
        public int ClassesNeededToReach75 { get; set; }
        public string? BilingualAlertMessage { get; set; }
        public string? WhatsAppUrl { get; set; }
    }

    public interface IAttendanceComplianceService
    {
        Task<AttendanceEvaluationResult> ProcessStudentAttendanceAsync(
            StudentProfile student,
            int courseId,
            DateTime targetDate,
            string status,
            int teacherStaffId);

        Task<List<AttendanceEvaluationResult>> ProcessBulkClassAttendanceAsync(
            int courseId,
            DateTime targetDate,
            Dictionary<int, string> studentStatuses,
            int teacherStaffId);
    }

    public class AttendanceComplianceService : IAttendanceComplianceService
    {
        private readonly ApplicationDbContext _context;
        private readonly IParentNotificationService _notificationService;
        private readonly ILogger<AttendanceComplianceService> _logger;

        public AttendanceComplianceService(
            ApplicationDbContext context,
            IParentNotificationService notificationService,
            ILogger<AttendanceComplianceService> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<AttendanceEvaluationResult> ProcessStudentAttendanceAsync(
            StudentProfile student,
            int courseId,
            DateTime targetDate,
            string status,
            int teacherStaffId)
        {
            var result = new AttendanceEvaluationResult
            {
                StudentId = student.Id,
                RollNumber = student.RollNumber,
                FullName = student.FullName,
                FatherName = student.FatherName,
                Phone = student.GuardianContact,
                ProgramLevel = student.Program?.Level ?? "HSSC"
            };

            var course = await _context.Courses
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == courseId);

            string courseTitle = course != null ? $"{course.CourseCode} - {course.Title}" : "Academic Subject";

            // Enforce Rustication Lock: If already rusticated, must remain Absent
            string effectiveStatus = status;
            if (student.AcademicStatus == AcademicStatus.Rusticated_ParentRequired)
            {
                effectiveStatus = "Absent";
            }

            // Save or update today's attendance record
            var existingRecord = await _context.AttendanceRecords
                .FirstOrDefaultAsync(a => a.StudentId == student.Id && a.CourseId == courseId && a.Date == targetDate);

            if (existingRecord != null)
            {
                existingRecord.Status = effectiveStatus;
                existingRecord.MarkedByStaffId = teacherStaffId;
            }
            else
            {
                var newRec = new AttendanceRecord
                {
                    StudentId = student.Id,
                    CourseId = courseId,
                    Date = targetDate,
                    Status = effectiveStatus,
                    MarkedByStaffId = teacherStaffId
                };
                await _context.AttendanceRecords.AddAsync(newRec);
            }

            // =========================================================================
            // RULE A: HSSC / F.Sc 3-Day Consecutive Absence Rustication Rule
            // =========================================================================
            if (student.Program?.Level == "HSSC" || student.Program?.ProgramLevel == ProgramLevel.HSSC_FSc)
            {
                if (effectiveStatus == "Absent")
                {
                    // Check previous 2 consecutive class days before targetDate
                    var pastRecords = await _context.AttendanceRecords
                        .AsNoTracking()
                        .Where(a => a.StudentId == student.Id && a.CourseId == courseId && a.Date < targetDate)
                        .OrderByDescending(a => a.Date)
                        .Take(2)
                        .ToListAsync();

                    bool wasAbsentD1 = pastRecords.Count > 0 && pastRecords[0].Status == "Absent";
                    bool wasAbsentD2 = pastRecords.Count > 1 && pastRecords[1].Status == "Absent";

                    if (wasAbsentD1 && wasAbsentD2)
                    {
                        // 3 Consecutive Absences Triggered!
                        if (student.AcademicStatus != AcademicStatus.Rusticated_ParentRequired &&
                            student.AcademicStatus != AcademicStatus.StruckOff)
                        {
                            student.AcademicStatus = AcademicStatus.Rusticated_ParentRequired;
                            student.ConsecutiveAbsentDays = 3;

                            string d1 = pastRecords[1].Date.ToString("dd MMM");
                            string d2 = pastRecords[0].Date.ToString("dd MMM");
                            string d3 = targetDate.ToString("dd MMM");
                            string datesSummary = $"{d1}, {d2}, {d3}";

                            var rustication = new RusticationRecord
                            {
                                StudentId = student.Id,
                                RusticationDate = DateTime.UtcNow,
                                AbsentDatesSummary = datesSummary,
                                Reason = $"Marked absent for 3 consecutive working/class days without approved leave ({datesSummary})",
                                CourseId = courseId,
                                IsReinstated = false
                            };
                            await _context.RusticationRecords.AddAsync(rustication);

                            // Generate Bilingual WhatsApp & SMS Message
                            string msg = _notificationService.GenerateFsc3DayRusticationMessage(
                                student.FatherName,
                                student.FullName,
                                student.RollNumber,
                                student.Gender,
                                datesSummary);

                            string waUrl = _notificationService.BuildWhatsAppUrl(student.GuardianContact, msg);
                            await _notificationService.LogNotificationAsync(student.Id, "FSc_3DayRustication", student.GuardianContact, msg);

                            result.TriggeredRustication = true;
                            result.RusticationDatesSummary = datesSummary;
                            result.BilingualAlertMessage = msg;
                            result.WhatsAppUrl = waUrl;

                            _logger.LogWarning("3-DAY RUSTICATION TRIGGERED: {Roll} ({Name}) rusticated. Absent dates: {Dates}.",
                                student.RollNumber, student.FullName, datesSummary);
                        }
                    }
                    else
                    {
                        if (student.AcademicStatus == AcademicStatus.Active)
                        {
                            student.ConsecutiveAbsentDays = Math.Min(student.ConsecutiveAbsentDays + 1, 2);
                        }
                    }
                }
                else if (effectiveStatus == "Present")
                {
                    if (student.AcademicStatus == AcademicStatus.Active)
                    {
                        student.ConsecutiveAbsentDays = 0;
                    }
                }
            }
            // =========================================================================
            // RULE B: BS 4-Year 75% Attendance Compliance Rule
            // =========================================================================
            else if (student.Program?.Level == "BS" || student.Program?.ProgramLevel == ProgramLevel.Undergraduate_BS)
            {
                // Calculate live attendance percentage for this course
                var allRecords = await _context.AttendanceRecords
                    .AsNoTracking()
                    .Where(a => a.StudentId == student.Id && a.CourseId == courseId)
                    .ToListAsync();

                // Incorporate today's status
                int totalConducted = allRecords.Count + (existingRecord == null ? 1 : 0);
                int presentCount = allRecords.Count(a => a.Status == "Present" && a.Date != targetDate) + (effectiveStatus == "Present" ? 1 : 0);
                int leaveCount = allRecords.Count(a => a.Status == "Leave" && a.Date != targetDate) + (effectiveStatus == "Leave" ? 1 : 0);
                int effectiveAttended = presentCount + leaveCount;

                decimal pct = totalConducted > 0 
                    ? Math.Round(((decimal)effectiveAttended / totalConducted) * 100m, 1) 
                    : 100m;

                result.BsAttendancePercentage = pct;

                if (pct < 75.0m && totalConducted > 0)
                {
                    result.ClassesNeededToReach75 = Math.Max(0, (3 * totalConducted) - (4 * effectiveAttended));
                }

                // If held >= 10 lectures and attendance < 75%
                if (totalConducted >= 10 && pct < 75.0m)
                {
                    // Check if student has Principal Condonation
                    bool isCondoned = await _context.AttendanceCondonations
                        .AsNoTracking()
                        .AnyAsync(c => c.StudentId == student.Id && c.CourseId == courseId && c.IsApprovedByPrincipal);

                    if (!isCondoned)
                    {
                        result.TriggeredBsDebarment = true;

                        string msg = _notificationService.GenerateBsAttendanceDebarmentMessage(
                            student.FatherName,
                            student.FullName,
                            student.RollNumber,
                            student.Gender,
                            courseTitle,
                            pct);

                        string waUrl = _notificationService.BuildWhatsAppUrl(student.GuardianContact, msg);
                        result.BilingualAlertMessage = msg;
                        result.WhatsAppUrl = waUrl;
                    }
                }
            }

            return result;
        }

        public async Task<List<AttendanceEvaluationResult>> ProcessBulkClassAttendanceAsync(
            int courseId,
            DateTime targetDate,
            Dictionary<int, string> studentStatuses,
            int teacherStaffId)
        {
            var results = new List<AttendanceEvaluationResult>();

            var students = await _context.StudentProfiles
                .Include(s => s.Program)
                .Where(s => studentStatuses.Keys.Contains(s.Id))
                .ToListAsync();

            foreach (var student in students)
            {
                if (studentStatuses.TryGetValue(student.Id, out var status))
                {
                    var eval = await ProcessStudentAttendanceAsync(student, courseId, targetDate, status, teacherStaffId);
                    results.Add(eval);
                }
            }

            await _context.SaveChangesAsync();
            return results;
        }
    }
}
