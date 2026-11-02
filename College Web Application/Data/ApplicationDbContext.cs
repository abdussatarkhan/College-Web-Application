using College_Web_Application.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace College_Web_Application.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Department> Departments => Set<Department>();
        public DbSet<AcademicProgram> AcademicPrograms => Set<AcademicProgram>();
        public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();
        public DbSet<StaffProfile> StaffProfiles => Set<StaffProfile>();
        public DbSet<Course> Courses => Set<Course>();
        public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
        public DbSet<ExamResult> ExamResults => Set<ExamResult>();
        public DbSet<FeeChallan> FeeChallans => Set<FeeChallan>();
        public DbSet<AdmissionApplication> AdmissionApplications => Set<AdmissionApplication>();
        public DbSet<Notice> Notices => Set<Notice>();
        public DbSet<TeacherCourseAssignment> TeacherCourseAssignments => Set<TeacherCourseAssignment>();
        public DbSet<RusticationRecord> RusticationRecords => Set<RusticationRecord>();
        public DbSet<AttendanceCondonation> AttendanceCondonations => Set<AttendanceCondonation>();
        public DbSet<ParentNotificationLog> ParentNotificationLogs => Set<ParentNotificationLog>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // StudentProfile configuration
            builder.Entity<StudentProfile>(entity =>
            {
                entity.HasIndex(s => s.RollNumber).IsUnique();
                entity.HasIndex(s => s.CNIC).IsUnique();
                entity.Property(s => s.CGPA).HasPrecision(4, 2);

                entity.HasOne(s => s.Program)
                      .WithMany(p => p.Students)
                      .HasForeignKey(s => s.ProgramId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // FeeChallan configuration
            builder.Entity<FeeChallan>(entity =>
            {
                entity.HasIndex(f => f.ChallanNumber).IsUnique();
                entity.Property(f => f.TuitionFee).HasPrecision(18, 2);
                entity.Property(f => f.AdmissionFee).HasPrecision(18, 2);
                entity.Property(f => f.ExamFee).HasPrecision(18, 2);
                entity.Property(f => f.LibraryFund).HasPrecision(18, 2);
                entity.Property(f => f.LabFund).HasPrecision(18, 2);
                entity.Property(f => f.LateFine).HasPrecision(18, 2);
                entity.Property(f => f.TotalAmount).HasPrecision(18, 2);

                entity.HasOne(f => f.Student)
                      .WithMany(s => s.FeeChallans)
                      .HasForeignKey(f => f.StudentId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // AdmissionApplication configuration
            builder.Entity<AdmissionApplication>(entity =>
            {
                entity.HasIndex(a => a.TrackingNumber).IsUnique();
                entity.Property(a => a.MatricObtainedMarks).HasPrecision(6, 2);
                entity.Property(a => a.MatricTotalMarks).HasPrecision(6, 2);
                entity.Property(a => a.HsscObtainedMarks).HasPrecision(6, 2);
                entity.Property(a => a.HsscTotalMarks).HasPrecision(6, 2);
                entity.Property(a => a.HafizMarks).HasPrecision(5, 2);
                entity.Property(a => a.MeritScore).HasPrecision(6, 2);

                entity.HasOne(a => a.AppliedProgram)
                      .WithMany(p => p.Applications)
                      .HasForeignKey(a => a.AppliedProgramId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // AttendanceRecord configuration
            builder.Entity<AttendanceRecord>(entity =>
            {
                entity.HasIndex(a => new { a.StudentId, a.CourseId, a.Date }).IsUnique();

                entity.HasOne(a => a.Student)
                      .WithMany(s => s.AttendanceRecords)
                      .HasForeignKey(a => a.StudentId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.Course)
                      .WithMany(c => c.AttendanceRecords)
                      .HasForeignKey(a => a.CourseId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ExamResult configuration
            builder.Entity<ExamResult>(entity =>
            {
                entity.Property(e => e.MidTermMarks).HasPrecision(5, 2);
                entity.Property(e => e.SessionalMarks).HasPrecision(5, 2);
                entity.Property(e => e.FinalMarks).HasPrecision(5, 2);
                entity.Property(e => e.TotalMarks).HasPrecision(5, 2);
                entity.Property(e => e.GradePoints).HasPrecision(4, 2);

                entity.HasOne(e => e.Student)
                      .WithMany(s => s.ExamResults)
                      .HasForeignKey(e => e.StudentId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Course)
                      .WithMany(c => c.ExamResults)
                      .HasForeignKey(e => e.CourseId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.SubmittedByStaff)
                      .WithMany()
                      .HasForeignKey(e => e.SubmittedByStaffId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // TeacherCourseAssignment configuration (Strict Subject-Level Security)
            builder.Entity<TeacherCourseAssignment>(entity =>
            {
                entity.HasIndex(t => new { t.StaffId, t.CourseId, t.AcademicSession, t.Section }).IsUnique();

                entity.HasOne(t => t.Staff)
                      .WithMany()
                      .HasForeignKey(t => t.StaffId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.Course)
                      .WithMany()
                      .HasForeignKey(t => t.CourseId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // RusticationRecord configuration (3-Day Consecutive Absence Discipline)
            builder.Entity<RusticationRecord>(entity =>
            {
                entity.Property(r => r.FinePaidAmount).HasPrecision(18, 2);

                entity.HasOne(r => r.Student)
                      .WithMany(s => s.RusticationRecords)
                      .HasForeignKey(r => r.StudentId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Course)
                      .WithMany()
                      .HasForeignKey(r => r.CourseId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // AttendanceCondonation configuration (BS 75% Compliance Waiver)
            builder.Entity<AttendanceCondonation>(entity =>
            {
                entity.Property(c => c.ActualAttendancePercentage).HasPrecision(5, 2);

                entity.HasOne(c => c.Student)
                      .WithMany()
                      .HasForeignKey(c => c.StudentId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.Course)
                      .WithMany()
                      .HasForeignKey(c => c.CourseId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ParentNotificationLog configuration (Bilingual English + Urdu Dispatch)
            builder.Entity<ParentNotificationLog>(entity =>
            {
                entity.HasOne(p => p.Student)
                      .WithMany(s => s.NotificationLogs)
                      .HasForeignKey(p => p.StudentId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // AcademicProgram configuration
            builder.Entity<AcademicProgram>(entity =>
            {
                entity.Property(p => p.FeePerSemester).HasPrecision(18, 2);

                entity.HasOne(p => p.Department)
                      .WithMany(d => d.Programs)
                      .HasForeignKey(p => p.DepartmentId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed initial college data
            SeedData(builder);
        }

        private static void SeedData(ModelBuilder builder)
        {
            // Seed Departments (Accredited by University of Swat & HED KP)
            builder.Entity<Department>().HasData(
                new Department { Id = 1, Name = "Computer Science", Code = "CS", HODName = "Prof. Dr. Muhammad Tariq Khan", Description = "Theoretical and applied computer science, algorithms, software engineering, web technologies, and AI." },
                new Department { Id = 2, Name = "Microbiology", Code = "MCB", HODName = "Assoc. Prof. Dr. Shaukat Khan", Description = "Bacteriology, virology, immunology, clinical microbiology, and diagnostic biotechnology." },
                new Department { Id = 3, Name = "English", Code = "ENG", HODName = "Asst. Prof. Zahir Shah", Description = "English literature, linguistics, global communication, rhetoric, and literary discourse." },
                new Department { Id = 4, Name = "Urdu", Code = "URD", HODName = "Assoc. Prof. Dr. Ahmad Ali", Description = "Classical and modern Urdu prose, poetry, linguistic history, and literary criticism." },
                new Department { Id = 5, Name = "Chemistry", Code = "CHEM", HODName = "Prof. Noor Muhammad", Description = "Organic, inorganic, analytical, physical chemistry, and environmental biochemistry." },
                new Department { Id = 6, Name = "Botany", Code = "BOT", HODName = "Asst. Prof. Dr. Fazal Wadood", Description = "Plant biodiversity of Swat Valley, medicinal flora, cellular genetics, and plant physiology." }
            );

            // Seed Academic Programs (BS 4-Year University of Swat + HSSC BISE Swat)
            builder.Entity<AcademicProgram>().HasData(
                // Intermediate Programs (HSSC - BISE Swat)
                new AcademicProgram { Id = 1, Title = "F.Sc Pre-Medical", Level = "HSSC", DurationYears = 2, TotalSemesters = 4, DepartmentId = 6, FeePerSemester = 3500.00m },
                new AcademicProgram { Id = 2, Title = "F.Sc Pre-Engineering", Level = "HSSC", DurationYears = 2, TotalSemesters = 4, DepartmentId = 5, FeePerSemester = 3500.00m },
                new AcademicProgram { Id = 3, Title = "FCS / ICS (Computer Science)", Level = "HSSC", DurationYears = 2, TotalSemesters = 4, DepartmentId = 1, FeePerSemester = 4000.00m },

                // BS 4-Year Degree Programs (Affiliated with University of Swat)
                new AcademicProgram { Id = 5, Title = "BS Computer Science (BSCS)", Level = "BS", DurationYears = 4, TotalSemesters = 8, DepartmentId = 1, FeePerSemester = 6500.00m },
                new AcademicProgram { Id = 6, Title = "BS Microbiology", Level = "BS", DurationYears = 4, TotalSemesters = 8, DepartmentId = 2, FeePerSemester = 6000.00m },
                new AcademicProgram { Id = 7, Title = "BS English Literature & Linguistics", Level = "BS", DurationYears = 4, TotalSemesters = 8, DepartmentId = 3, FeePerSemester = 5500.00m },
                new AcademicProgram { Id = 8, Title = "BS Urdu", Level = "BS", DurationYears = 4, TotalSemesters = 8, DepartmentId = 4, FeePerSemester = 4500.00m },
                new AcademicProgram { Id = 9, Title = "BS Chemistry", Level = "BS", DurationYears = 4, TotalSemesters = 8, DepartmentId = 5, FeePerSemester = 6000.00m },
                new AcademicProgram { Id = 10, Title = "BS Botany", Level = "BS", DurationYears = 4, TotalSemesters = 8, DepartmentId = 6, FeePerSemester = 5500.00m }
            );

            // Seed Sample Courses
            builder.Entity<Course>().HasData(
                new Course { Id = 1, CourseCode = "CS-101", Title = "Introduction to Computing & Programming", CreditHours = 4, ProgramId = 5, Semester = 1 },
                new Course { Id = 2, CourseCode = "MATH-101", Title = "Calculus & Analytical Geometry", CreditHours = 3, ProgramId = 5, Semester = 1 },
                new Course { Id = 3, CourseCode = "ENG-101", Title = "English Composition & Comprehension", CreditHours = 3, ProgramId = 5, Semester = 1 },
                new Course { Id = 4, CourseCode = "CS-201", Title = "Data Structures & Algorithms", CreditHours = 4, ProgramId = 5, Semester = 3 },
                new Course { Id = 5, CourseCode = "CS-301", Title = "Database Systems", CreditHours = 4, ProgramId = 5, Semester = 5 },
                new Course { Id = 6, CourseCode = "MCB-101", Title = "Fundamentals of Microbiology", CreditHours = 4, ProgramId = 6, Semester = 1 },
                new Course { Id = 7, CourseCode = "ENG-201", Title = "History of English Literature", CreditHours = 3, ProgramId = 7, Semester = 1 },
                new Course { Id = 8, CourseCode = "CHEM-101", Title = "Inorganic & Physical Chemistry", CreditHours = 4, ProgramId = 9, Semester = 1 }
            );

            // Seed Notices
            builder.Entity<Notice>().HasData(
                new Notice
                {
                    Id = 1,
                    Title = "Online Admissions Open for BS 4-Year Programs (Fall 2026 Session)",
                    Category = "Admission",
                    Description = "Applications are invited on online merit basis for BS Computer Science, Political Science, English, Islamic Studies, and Zoology.",
                    AttachmentPdfUrl = "/downloads/admissions-fall-2026.pdf",
                    PublishedDate = new DateTime(2026, 9, 1),
                    IsPinned = true
                },
                new Notice
                {
                    Id = 2,
                    Title = "BISE Swat HSSC Annual Examination 2026 - Date Sheet & Roll No Slips",
                    Category = "Exam",
                    Description = "Intermediate Part-I and Part-II annual exam date sheets issued by BISE Swat are now available for regular college students.",
                    AttachmentPdfUrl = "/downloads/bise-swat-datesheet-2026.pdf",
                    PublishedDate = new DateTime(2026, 9, 15),
                    IsPinned = true
                },
                new Notice
                {
                    Id = 3,
                    Title = "KP Chief Minister Educational Endowment Fund & MORA Scholarships",
                    Category = "Scholarship",
                    Description = "Deserving and high-merit students of Upper Swat/Madyan are informed to submit MORA and HED KP scholarship forms by the due date.",
                    AttachmentPdfUrl = "/downloads/scholarship-form-2026.pdf",
                    PublishedDate = new DateTime(2026, 10, 5),
                    IsPinned = true
                },
                new Notice
                {
                    Id = 4,
                    Title = "Semester Fee Submission Deadline for BS Fall 2026 - Bank of Khyber",
                    Category = "General",
                    Description = "All BS students are directed to deposit their semester tuition fee through official 3-copy challan at BoK Madyan branch without late fine.",
                    AttachmentPdfUrl = "/downloads/fee-schedule-2026.pdf",
                    PublishedDate = new DateTime(2026, 10, 10),
                    IsPinned = false
                }
            );

            // Seed Staff Profiles
            builder.Entity<StaffProfile>().HasData(
                new StaffProfile
                {
                    Id = 1,
                    EmployeeCode = "EMP-GDCM-001",
                    FullName = "Mr. Rasheed Ahmad",
                    Designation = "Principal / Director",
                    BPSGrade = "BPS-20",
                    Qualification = "M.Phil / Master in Educational Leadership & Higher Education Administration",
                    DepartmentId = null,
                    JoiningDate = new DateTime(2012, 4, 15),
                    PhotoUrl = "/images/principal-rasheed-ahmad.jpg",
                    IsActive = true
                },
                new StaffProfile
                {
                    Id = 2,
                    EmployeeCode = "EMP-GDCM-002",
                    FullName = "Prof. Dr. Muhammad Tariq Khan",
                    Designation = "Professor & HOD",
                    BPSGrade = "BPS-20",
                    Qualification = "Ph.D Computer Science (Distributed Systems)",
                    DepartmentId = 1,
                    JoiningDate = new DateTime(2014, 8, 1),
                    PhotoUrl = "/images/staff/hod-cs.jpg",
                    IsActive = true
                },
                new StaffProfile
                {
                    Id = 3,
                    EmployeeCode = "EMP-GDCM-003",
                    FullName = "Assoc. Prof. Fazal Rehman",
                    Designation = "Associate Professor",
                    BPSGrade = "BPS-19",
                    Qualification = "M.Phil Political Science",
                    DepartmentId = 2,
                    JoiningDate = new DateTime(2015, 3, 10),
                    PhotoUrl = "/images/staff/prof-pol.jpg",
                    IsActive = true
                },
                new StaffProfile
                {
                    Id = 4,
                    EmployeeCode = "EMP-GDCM-004",
                    FullName = "Mr. Irfan Ullah",
                    Designation = "Lecturer",
                    BPSGrade = "BPS-17",
                    Qualification = "MS Software Engineering",
                    DepartmentId = 1,
                    JoiningDate = new DateTime(2019, 11, 20),
                    PhotoUrl = "/images/staff/lect-cs.jpg",
                    IsActive = true
                },
                new StaffProfile
                {
                    Id = 5,
                    EmployeeCode = "EMP-GDCM-005",
                    FullName = "Mr. Sher Muhammad Khan",
                    Designation = "Senior Clerk",
                    BPSGrade = "BPS-14",
                    Qualification = "B.Com / Accounts & Administration",
                    DepartmentId = null,
                    JoiningDate = new DateTime(2010, 2, 1),
                    PhotoUrl = "/images/staff/clerk.jpg",
                    IsActive = true
                }
            );

            // Seed Sample Students
            builder.Entity<StudentProfile>().HasData(
                new StudentProfile
                {
                    Id = 1,
                    RollNumber = "GDCM-BSCS-2026-001",
                    RegistrationNo = "UOS-2026-CS-001",
                    FullName = "Muhammad Bilal Swati",
                    FatherName = "Fazal Wadood",
                    CNIC = "15602-1234567-1",
                    DateOfBirth = new DateTime(2005, 3, 14),
                    Phone = "0345-9876543",
                    DomicileDistrict = "Swat",
                    Address = "Main Bazaar, Madyan, District Swat, KP",
                    ProgramId = 5, // BSCS
                    CurrentSemester = 5,
                    Section = "A",
                    CGPA = 3.78m,
                    AdmissionDate = new DateTime(2024, 9, 1),
                    IsActive = true
                },
                new StudentProfile
                {
                    Id = 2,
                    RollNumber = "GDCM-BSCS-2026-002",
                    RegistrationNo = "UOS-2026-CS-002",
                    FullName = "Sohail Ahmad",
                    FatherName = "Said Karim",
                    CNIC = "15602-7654321-3",
                    DateOfBirth = new DateTime(2004, 7, 21),
                    Phone = "0301-8765432",
                    DomicileDistrict = "Swat",
                    Address = "Village Chail, Madyan, District Swat, KP",
                    ProgramId = 5, // BSCS
                    CurrentSemester = 5,
                    Section = "A",
                    CGPA = 3.62m,
                    AdmissionDate = new DateTime(2024, 9, 1),
                    IsActive = true
                },
                new StudentProfile
                {
                    Id = 3,
                    RollNumber = "GDCM-PM-2026-015",
                    RegistrationNo = "BISE-SWAT-2026-9021",
                    FullName = "Hamza Ali",
                    FatherName = "Bakht Rawan",
                    CNIC = "15602-9812345-5",
                    DateOfBirth = new DateTime(2008, 1, 10),
                    Phone = "0333-9123456",
                    DomicileDistrict = "Swat",
                    Address = "Near Bridge, Bahrain, District Swat, KP",
                    ProgramId = 1, // F.Sc Pre-Med
                    CurrentSemester = 2,
                    Section = "B",
                    CGPA = 3.85m,
                    AdmissionDate = new DateTime(2025, 9, 1),
                    IsActive = true
                },
                new StudentProfile
                {
                    Id = 4,
                    RollNumber = "GDCM-POL-2026-004",
                    RegistrationNo = "UOS-2026-POL-044",
                    FullName = "Aftab Khan",
                    FatherName = "Gul Zada",
                    CNIC = "15602-5544332-7",
                    DateOfBirth = new DateTime(2005, 11, 5),
                    Phone = "0312-3456789",
                    DomicileDistrict = "Swat",
                    Address = "Fatehpur, District Swat, KP",
                    ProgramId = 6, // BS Pol Sci
                    CurrentSemester = 3,
                    Section = "A",
                    CGPA = 3.40m,
                    AdmissionDate = new DateTime(2025, 9, 1),
                    IsActive = true
                }
            );

            // Seed Sample Fee Challans
            builder.Entity<FeeChallan>().HasData(
                new FeeChallan
                {
                    Id = 1,
                    ChallanNumber = "BOK-GDCM-2026-1001",
                    StudentId = 1,
                    Semester = 5,
                    TuitionFee = 4500.00m,
                    AdmissionFee = 0.00m,
                    ExamFee = 1500.00m,
                    LibraryFund = 500.00m,
                    LabFund = 500.00m,
                    LateFine = 0.00m,
                    TotalAmount = 7000.00m,
                    IssueDate = new DateTime(2026, 9, 1),
                    DueDate = new DateTime(2026, 9, 25),
                    IsPaid = true,
                    PaidDate = new DateTime(2026, 9, 18),
                    BankReference = "BOK-MDY-DEP-8841"
                },
                new FeeChallan
                {
                    Id = 2,
                    ChallanNumber = "BOK-GDCM-2026-1002",
                    StudentId = 2,
                    Semester = 5,
                    TuitionFee = 4500.00m,
                    AdmissionFee = 0.00m,
                    ExamFee = 1500.00m,
                    LibraryFund = 500.00m,
                    LabFund = 500.00m,
                    LateFine = 0.00m,
                    TotalAmount = 7000.00m,
                    IssueDate = new DateTime(2026, 9, 1),
                    DueDate = new DateTime(2026, 9, 25),
                    IsPaid = false,
                    PaidDate = null,
                    BankReference = null
                },
                new FeeChallan
                {
                    Id = 3,
                    ChallanNumber = "BOK-GDCM-2026-1003",
                    StudentId = 3,
                    Semester = 2,
                    TuitionFee = 2500.00m,
                    AdmissionFee = 0.00m,
                    ExamFee = 1000.00m,
                    LibraryFund = 500.00m,
                    LabFund = 500.00m,
                    LateFine = 0.00m,
                    TotalAmount = 4500.00m,
                    IssueDate = new DateTime(2026, 9, 1),
                    DueDate = new DateTime(2026, 9, 25),
                    IsPaid = true,
                    PaidDate = new DateTime(2026, 9, 20),
                    BankReference = "BOK-MDY-DEP-9104"
                }
            );

            // Seed Sample Admission Applications (Ready to test merit list and enrollment)
            builder.Entity<AdmissionApplication>().HasData(
                new AdmissionApplication
                {
                    Id = 1,
                    TrackingNumber = "GDCM-ADM-2026-0001",
                    ApplicantName = "Usman Ghani",
                    FatherName = "Abdul Qayum",
                    CNIC = "15602-3344556-9",
                    Gender = "Male",
                    BloodGroup = "B+",
                    DateOfBirth = new DateTime(2007, 4, 12),
                    Phone = "0346-1234567",
                    GuardianPhone = "0345-7654321",
                    DomicileDistrict = "Swat",
                    Tehsil = "Bahrain / Madyan",
                    PostalAddress = "Village Ashuran, Madyan, District Swat",
                    SscBoard = "BISE Swat",
                    SscRollNumber = "451204",
                    SscPassingYear = 2024,
                    MatricObtainedMarks = 980.00m,
                    MatricTotalMarks = 1100.00m,
                    HsscBoard = "BISE Swat",
                    HsscRollNumber = "889012",
                    HsscPassingYear = 2026,
                    HsscObtainedMarks = 945.00m,
                    HsscTotalMarks = 1100.00m,
                    IsHafizQuran = true,
                    HafizMarks = 20.00m,
                    MeritScore = 965.00m, // Marks-based merit: 945 HSSC obtained + 20 Hafiz
                    AppliedProgramId = 5, // BSCS
                    SecondChoiceProgramId = 6, // BS Pol Sci
                    QuotaCategory = "Open Merit",
                    Status = "MeritListed",
                    AppliedDate = new DateTime(2026, 9, 5)
                },
                new AdmissionApplication
                {
                    Id = 2,
                    TrackingNumber = "GDCM-ADM-2026-0002",
                    ApplicantName = "Jawad Ahmad",
                    FatherName = "Akhtar Ali",
                    CNIC = "15602-6677889-1",
                    Gender = "Male",
                    BloodGroup = "O+",
                    DateOfBirth = new DateTime(2007, 8, 25),
                    Phone = "0347-8901234",
                    GuardianPhone = "0300-1122334",
                    DomicileDistrict = "Swat",
                    Tehsil = "Bahrain / Madyan",
                    PostalAddress = "Madyan Bazaar, Swat",
                    SscBoard = "BISE Swat",
                    SscRollNumber = "451205",
                    SscPassingYear = 2024,
                    MatricObtainedMarks = 910.00m,
                    MatricTotalMarks = 1100.00m,
                    HsscBoard = "BISE Swat",
                    HsscRollNumber = "889015",
                    HsscPassingYear = 2026,
                    HsscObtainedMarks = 890.00m,
                    HsscTotalMarks = 1100.00m,
                    IsHafizQuran = false,
                    HafizMarks = 0.00m,
                    MeritScore = 890.00m, // Marks-based merit: 890 HSSC obtained
                    AppliedProgramId = 5, // BSCS
                    QuotaCategory = "Open Merit",
                    Status = "Pending",
                    AppliedDate = new DateTime(2026, 9, 6)
                }
            );
        }
    }
}
