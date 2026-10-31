using College_Web_Application.Models;

namespace College_Web_Application.Models.ViewModels
{
    public class RusticatedStudentItemViewModel
    {
        public int RecordId { get; set; }
        public int StudentId { get; set; }
        public string RollNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string FatherName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string ProgramTitle { get; set; } = string.Empty;
        public string Level { get; set; } = "HSSC";
        public string Section { get; set; } = "A";
        public DateTime RusticationDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public bool IsReinstated { get; set; }
        public string? ParentNameVisited { get; set; }
        public string? ParentCNIC { get; set; }
        public string? ParentUndertakingNotes { get; set; }
        public DateTime? ReinstatedDate { get; set; }
        public decimal FinePaidAmount { get; set; }
        public string? AbsentDatesSummary { get; set; }

        public string WhatsAppUrl
        {
            get
            {
                string clean = Phone.Replace("-", "").Replace(" ", "").Trim();
                if (clean.StartsWith("03"))
                {
                    clean = "92" + clean.Substring(1);
                }
                else if (!clean.StartsWith("92"))
                {
                    clean = "92" + clean;
                }

                string dates = string.IsNullOrEmpty(AbsentDatesSummary) ? "past 3 consecutive class days" : AbsentDatesSummary;

                string notice = $"*GOVERNMENT DEGREE COLLEGE MADYAN SWAT*\n" +
                               $"Dear {FatherName}, parent of {FullName} (Roll No: {RollNumber}), your son has been marked continuously absent for 3 days ({dates}), due to which you are informed that your son is rusticated from Government Degree College Madyan Swat. Your son is not allowed to sit in the class until you personally come to Government Degree College Madyan Swat with him and meet the Principal / Discipline Committee.\n\n" +
                               $"*گورنمنٹ ڈگری کالج مدین سوات — ضروری اطلاعی نوٹس*\n" +
                               $"محترم {FatherName} صاحب (والد/سرپرستِ {FullName}، رول نمبر: {RollNumber})، آپ کو مطلع کیا جاتا ہے کہ آپ کے بیٹے مسلسل 3 دن ({dates}) سے غیر حاضر ہیں، جس کی وجہ سے آپ کو اطلاع دی جاتی ہے کہ آپ کے بیٹے کو گورنمنٹ ڈگری کالج مدین سوات سے خارج (Rusticate) کر دیا گیا ہے۔ لہٰذا آپ فوری طور پر اپنے بیٹے کے ہمراہ گورنمنٹ ڈگری کالج مدین سوات تشریف لائیں اور پرنسپل/ڈسپلن کمیٹی سے ملاقات کریں، تب تک طالب علم کو کلاس میں بیٹھنے کی اجازت نہیں ہوگی۔";

                return $"https://wa.me/{clean}?text={Uri.EscapeDataString(notice)}";
            }
        }
    }

    public class ReinstateStudentRequestModel
    {
        public int RecordId { get; set; }
        public string ParentNameVisited { get; set; } = string.Empty;
        public string ParentCNIC { get; set; } = string.Empty;
        public string ParentUndertakingNotes { get; set; } = string.Empty;
        public decimal FinePaidAmount { get; set; } = 0.00m;
    }

    public class AttendanceSummaryFilterViewModel
    {
        public int? SelectedProgramId { get; set; }
        public string SelectedSection { get; set; } = "A";
        public List<AcademicProgram> Programs { get; set; } = new();
        public List<StudentAttendanceSummaryRow> Rows { get; set; } = new();
    }

    public class StudentAttendanceSummaryRow
    {
        public int StudentId { get; set; }
        public string RollNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string FatherName { get; set; } = string.Empty;
        public string ProgramTitle { get; set; } = string.Empty;
        public int TotalClassesHeld { get; set; }
        public int TotalPresent { get; set; }
        public int TotalAbsent { get; set; }
        public int TotalLeave { get; set; }
        public decimal AttendancePercentage => TotalClassesHeld > 0 ? Math.Round((decimal)TotalPresent * 100m / TotalClassesHeld, 1) : 0m;
        public int ConsecutiveAbsentStreak { get; set; }
        public AcademicStatus AcademicStatus { get; set; }
    }
}
