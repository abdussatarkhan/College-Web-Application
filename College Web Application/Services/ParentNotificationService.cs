using College_Web_Application.Data;
using College_Web_Application.Models;
using Microsoft.EntityFrameworkCore;

namespace College_Web_Application.Services
{
    public interface IParentNotificationService
    {
        string GenerateFsc3DayRusticationMessage(string fatherName, string studentName, string rollNumber, string gender, string absentDates);
        string GenerateBsAttendanceDebarmentMessage(string fatherName, string studentName, string rollNumber, string gender, string courseTitle, decimal attendancePct);
        string BuildWhatsAppUrl(string phoneNumber, string message);
        Task<ParentNotificationLog> LogNotificationAsync(int studentId, string alertType, string phone, string message, string? dispatchedBy = null);
    }

    public class ParentNotificationService : IParentNotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ParentNotificationService> _logger;

        public ParentNotificationService(ApplicationDbContext context, ILogger<ParentNotificationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public string GenerateFsc3DayRusticationMessage(string fatherName, string studentName, string rollNumber, string gender, string absentDates)
        {
            bool isFemale = gender.Equals("Female", StringComparison.OrdinalIgnoreCase);
            string childEn = isFemale ? "daughter" : "son";
            string pronounEn = isFemale ? "her" : "him";
            string childUr = isFemale ? "بیٹی" : "بیٹے";

            return $"GOVERNMENT DEGREE COLLEGE MADYAN SWAT\n" +
                   $"Dear {fatherName}, parent of {studentName} (Roll No: {rollNumber}), your {childEn} has been marked continuously absent for 3 days ({absentDates}), due to which you are informed that your {childEn} is rusticated from Government Degree College Madyan Swat. Your {childEn} is not allowed to sit in the class until you personally come to Government Degree College Madyan Swat with {pronounEn} and meet the Principal / Discipline Committee.\n\n" +
                   $"گورنمنٹ ڈگری کالج مدین سوات — ضروری اطلاعی نوٹس\n" +
                   $"محترم {fatherName} صاحب (والد/سرپرستِ {studentName}، رول نمبر: {rollNumber})، آپ کو مطلع کیا جاتا ہے کہ آپ کے {childUr} مسلسل 3 دن ({absentDates}) سے غیر حاضر ہیں، جس کی وجہ سے آپ کو اطلاع دی جاتی ہے کہ آپ کے {childUr} کو گورنمنٹ ڈگری کالج مدین سوات سے خارج (Rusticate) کر دیا گیا ہے۔ لہٰذا آپ فوری طور پر اپنے {childUr} کے ہمراہ گورنمنٹ ڈگری کالج مدین سوات تشریف لائیں اور پرنسپل/ڈسپلن کمیٹی سے ملاقات کریں، تب تک طالب علم کو کلاس میں بیٹھنے کی اجازت نہیں ہوگی۔";
        }

        public string GenerateBsAttendanceDebarmentMessage(string fatherName, string studentName, string rollNumber, string gender, string courseTitle, decimal attendancePct)
        {
            bool isFemale = gender.Equals("Female", StringComparison.OrdinalIgnoreCase);
            string childEn = isFemale ? "daughter" : "son";
            string possEn = isFemale ? "her" : "his";
            string childUr = isFemale ? "بیٹی" : "بیٹے";

            return $"GOVERNMENT DEGREE COLLEGE MADYAN SWAT\n" +
                   $"Dear {fatherName}, parent of {studentName} (Roll No: {rollNumber}), your {childEn}'s absences are greater than 25% (Current Attendance: {attendancePct:0.#}% in {courseTitle}), due to which it is to inform you that your {childEn} will not be allowed to sit in the exams at Government Degree College Madyan Swat. Please make sure {possEn} attendance in the class is regular and accurate immediately.\n\n" +
                   $"گورنمنٹ ڈگری کالج مدین سوات — تنبیہی نوٹس برائے امتحانات و حاضری\n" +
                   $"محترم {fatherName} صاحب (والد/سرپرستِ {studentName}، رول نمبر: {rollNumber})، آپ کو مطلع کیا جاتا ہے کہ گورنمنٹ ڈگری کالج مدین سوات میں آپ کے {childUr} کی غیر حاضریوں کی شرح 25 فیصد سے بڑھ چکی ہے (مضمون {courseTitle} میں موجودہ حاضری: {attendancePct:0.#}%)، جس کی بنا پر آپ کو اطلاع دی جاتی ہے کہ آپ کے {childUr} کو گورنمنٹ ڈگری کالج مدین سوات کے امتحانات میں بیٹھنے کی اجازت نہیں دی جائے گی۔ براہِ کرم اپنے {childUr} کی کلاس میں حاضری کو فوری طور پر درست اور باقاعدہ بنائیں۔";
        }

        public string BuildWhatsAppUrl(string phoneNumber, string message)
        {
            string cleanPhone = new string((phoneNumber ?? "923000000000").Where(char.IsDigit).ToArray());
            if (!cleanPhone.StartsWith("92") && cleanPhone.StartsWith("0"))
            {
                cleanPhone = "92" + cleanPhone.Substring(1);
            }
            else if (!cleanPhone.StartsWith("92"))
            {
                cleanPhone = "92" + cleanPhone;
            }

            return $"https://wa.me/{cleanPhone}?text={Uri.EscapeDataString(message)}";
        }

        public async Task<ParentNotificationLog> LogNotificationAsync(int studentId, string alertType, string phone, string message, string? dispatchedBy = null)
        {
            var log = new ParentNotificationLog
            {
                StudentId = studentId,
                AlertType = alertType,
                ParentPhone = phone,
                BilingualMessage = message,
                SentAt = DateTime.UtcNow,
                IsDelivered = true,
                DispatchedByUserId = dispatchedBy ?? "System / Attendance Engine"
            };

            await _context.ParentNotificationLogs.AddAsync(log);
            await _context.SaveChangesAsync();

            _logger.LogInformation("PARENT NOTIFICATION DISPATCHED: StudentId={StudentId}, AlertType={AlertType}, Phone={Phone}",
                studentId, alertType, phone);

            return log;
        }
    }
}
