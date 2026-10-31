using College_Web_Application.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace College_Web_Application.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

            try
            {
                // 1. Ensure Database & Schema are Created
                if (context.Database.IsRelational())
                {
                    await context.Database.EnsureCreatedAsync();
                }
                else
                {
                    await context.Database.EnsureCreatedAsync();
                }

                // 2. Seed Institutional Roles
                string[] roles = ["SuperAdmin", "Principal", "Clerk", "HOD", "Faculty", "Student", "ControllerExams"];
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }

                // 3. Seed 6 Official Departments if not existing
                if (!await context.Departments.AnyAsync())
                {
                    var departments = new List<Department>
                    {
                        new() { Id = 1, Name = "Computer Science", Code = "CS", HODName = "Prof. Dr. Muhammad Tariq Khan", Description = "Theoretical and applied computer science, software engineering, algorithms, web technologies, and AI." },
                        new() { Id = 2, Name = "Political Science", Code = "POL", HODName = "Assoc. Prof. Fazal Rehman", Description = "Constitutional law, public administration, international politics, and regional diplomacy." },
                        new() { Id = 3, Name = "English", Code = "ENG", HODName = "Asst. Prof. Zahir Shah", Description = "English literature, linguistics, global communication, and critical discourse." },
                        new() { Id = 4, Name = "Islamic Studies", Code = "ISL", HODName = "Prof. Maulana Abdul Haq", Description = "Quranic sciences, Hadith literature, Islamic jurisprudence (Fiqh), and moral philosophy." },
                        new() { Id = 5, Name = "Biological Sciences (Pre-Medical)", Code = "BIO", HODName = "Assoc. Prof. Dr. Shaukat Khan", Description = "Botany, Zoology, cellular biology, genetics, and biodiversity of the upper Swat ecosystem." },
                        new() { Id = 6, Name = "Physical Sciences (Pre-Engineering)", Code = "PHY-CHEM", HODName = "Prof. Noor Muhammad", Description = "Experimental Physics, organic and inorganic chemistry, applied mathematics, and mechanics." }
                    };
                    await context.Departments.AddRangeAsync(departments);
                    await context.SaveChangesAsync();
                }

                // 4. Seed 6 Academic Programs if not existing
                if (!await context.AcademicPrograms.AnyAsync())
                {
                    var programs = new List<AcademicProgram>
                    {
                        new() { Id = 1, Title = "F.Sc Pre-Medical", Level = "HSSC", DurationYears = 2, TotalSemesters = 4, DepartmentId = 5, FeePerSemester = 3500.00m },
                        new() { Id = 2, Title = "F.Sc Pre-Engineering", Level = "HSSC", DurationYears = 2, TotalSemesters = 4, DepartmentId = 6, FeePerSemester = 3500.00m },
                        new() { Id = 3, Title = "FCS / ICS (Computer Science)", Level = "HSSC", DurationYears = 2, TotalSemesters = 4, DepartmentId = 1, FeePerSemester = 4000.00m },
                        new() { Id = 4, Title = "BS Computer Science (4-Year)", Level = "BS", DurationYears = 4, TotalSemesters = 8, DepartmentId = 1, FeePerSemester = 6500.00m },
                        new() { Id = 5, Title = "BS Political Science (4-Year)", Level = "BS", DurationYears = 4, TotalSemesters = 8, DepartmentId = 2, FeePerSemester = 5000.00m },
                        new() { Id = 6, Title = "BS English (4-Year)", Level = "BS", DurationYears = 4, TotalSemesters = 8, DepartmentId = 3, FeePerSemester = 5500.00m }
                    };
                    await context.AcademicPrograms.AddRangeAsync(programs);
                    await context.SaveChangesAsync();
                }

                // 5. Seed Core Academic Courses
                if (!await context.Courses.AnyAsync())
                {
                    var courses = new List<Course>
                    {
                        new() { Id = 1, CourseCode = "CS-101", Title = "Introduction to Computing & Programming", CreditHours = 4, ProgramId = 4, Semester = 1 },
                        new() { Id = 2, CourseCode = "CS-102", Title = "Digital Logic Design", CreditHours = 3, ProgramId = 4, Semester = 1 },
                        new() { Id = 3, CourseCode = "MATH-101", Title = "Calculus & Analytical Geometry", CreditHours = 3, ProgramId = 4, Semester = 1 },
                        new() { Id = 4, CourseCode = "ENG-101", Title = "English Composition & Rhetoric", CreditHours = 3, ProgramId = 4, Semester = 1 },
                        new() { Id = 5, CourseCode = "CS-201", Title = "Data Structures & Algorithms", CreditHours = 4, ProgramId = 4, Semester = 3 },
                        new() { Id = 6, CourseCode = "CS-301", Title = "Database Systems & Design", CreditHours = 4, ProgramId = 4, Semester = 5 },
                        new() { Id = 7, CourseCode = "CS-302", Title = "Operating Systems Principles", CreditHours = 4, ProgramId = 4, Semester = 5 },
                        new() { Id = 8, CourseCode = "CS-303", Title = "Web Technologies & ASP.NET Core", CreditHours = 3, ProgramId = 4, Semester = 5 },
                        new() { Id = 9, CourseCode = "POL-101", Title = "Introduction to Political Science", CreditHours = 3, ProgramId = 5, Semester = 1 },
                        new() { Id = 10, CourseCode = "ENG-201", Title = "History of English Literature", CreditHours = 3, ProgramId = 6, Semester = 1 },
                        new() { Id = 11, CourseCode = "BIO-101", Title = "Cell Biology & Physiology", CreditHours = 4, ProgramId = 1, Semester = 1 },
                        new() { Id = 12, CourseCode = "PHY-101", Title = "Mechanics & Wave Optics", CreditHours = 4, ProgramId = 2, Semester = 1 }
                    };
                    await context.Courses.AddRangeAsync(courses);
                    await context.SaveChangesAsync();
                }

                // 6. Seed 5 Official Institutional Pinned Notices
                if (!await context.Notices.AnyAsync())
                {
                    var notices = new List<Notice>
                    {
                        new()
                        {
                            Id = 1,
                            Title = "Online Admissions Open for BS 4-Year Programs (Fall 2026 Session)",
                            Category = "Admission",
                            Description = "Government Degree College Madyan Swat invites online admission applications for BS Computer Science, Political Science, and English. Applicants can submit their forms online with SSC/HSSC records.",
                            AttachmentPdfUrl = "/downloads/admissions-fall-2026.pdf",
                            PublishedDate = DateTime.UtcNow.AddDays(-15),
                            IsPinned = true
                        },
                        new()
                        {
                            Id = 2,
                            Title = "BISE Swat HSSC Annual Examinations 2026 - Date Sheet & Roll No Slips",
                            Category = "Exam",
                            Description = "All Intermediate Part-I & Part-II (F.Sc, ICS, FA) students are informed that roll number slips and date sheets issued by BISE Swat are now available at the college examination office.",
                            AttachmentPdfUrl = "/downloads/bise-swat-datesheet-2026.pdf",
                            PublishedDate = DateTime.UtcNow.AddDays(-10),
                            IsPinned = true
                        },
                        new()
                        {
                            Id = 3,
                            Title = "KP Chief Minister Educational Endowment & MORA Scholarships",
                            Category = "Scholarship",
                            Description = "Applications are invited from high-merit and financially deserving students of Upper Swat/Madyan for HED KP MORA and provincial endowment scholarships.",
                            AttachmentPdfUrl = "/downloads/scholarship-form-2026.pdf",
                            PublishedDate = DateTime.UtcNow.AddDays(-6),
                            IsPinned = true
                        },
                        new()
                        {
                            Id = 4,
                            Title = "University of Swat BS Fall 2026 Semester Fee Submission via BoK",
                            Category = "General",
                            Description = "All enrolled BS students must deposit their semester tuition fee through the official 3-copy Bank of Khyber (BoK) challan at Madyan Branch before the due date to avoid late fines.",
                            AttachmentPdfUrl = "/downloads/fee-schedule-2026.pdf",
                            PublishedDate = DateTime.UtcNow.AddDays(-3),
                            IsPinned = true
                        },
                        new()
                        {
                            Id = 5,
                            Title = "Academic Class Time Table & Lab Rotation Schedule (Fall 2026)",
                            Category = "TimeTable",
                            Description = "The master class time table and computer/science laboratory practical schedule for all semesters has been published by the academic committee.",
                            AttachmentPdfUrl = "/downloads/class-timetable-2026.pdf",
                            PublishedDate = DateTime.UtcNow.AddDays(-1),
                            IsPinned = true
                        }
                    };
                    await context.Notices.AddRangeAsync(notices);
                    await context.SaveChangesAsync();
                }

                // 7. Seed Staff Profiles
                if (!await context.StaffProfiles.AnyAsync())
                {
                    var staffList = new List<StaffProfile>
                    {
                        new()
                        {
                            Id = 1,
                            EmployeeCode = "EMP-GDCM-001",
                            FullName = "Mr. Rasheed Ahmad",
                            Designation = "Principal / Director",
                            BPSGrade = "BPS-20",
                            Qualification = "M.Phil / Master in Educational Leadership & Higher Education Administration",
                            DepartmentId = null,
                            JoiningDate = new DateTime(2010, 4, 15),
                            PhotoUrl = "/images/principal-rasheed-ahmad.jpg",
                            IsActive = true
                        },
                        new()
                        {
                            Id = 2,
                            EmployeeCode = "EMP-GDCM-002",
                            FullName = "Prof. Dr. Muhammad Tariq Khan",
                            Designation = "Professor & HOD",
                            BPSGrade = "BPS-20",
                            Qualification = "Ph.D Computer Science (Distributed Systems & Cloud Computing)",
                            DepartmentId = 1,
                            JoiningDate = new DateTime(2013, 8, 1),
                            PhotoUrl = "/images/staff/dr-tariq.jpg",
                            IsActive = true
                        },
                        new()
                        {
                            Id = 3,
                            EmployeeCode = "EMP-GDCM-003",
                            FullName = "Assoc. Prof. Fazal Rehman",
                            Designation = "Associate Professor",
                            BPSGrade = "BPS-19",
                            Qualification = "M.Phil Political Science (KP Public Policy & Governance)",
                            DepartmentId = 2,
                            JoiningDate = new DateTime(2015, 3, 10),
                            PhotoUrl = "/images/staff/fazal-rehman.jpg",
                            IsActive = true
                        },
                        new()
                        {
                            Id = 4,
                            EmployeeCode = "EMP-GDCM-004",
                            FullName = "Mr. Irfan Ullah",
                            Designation = "Lecturer",
                            BPSGrade = "BPS-17",
                            Qualification = "MS Software Engineering (FAST-NUCES / UOS)",
                            DepartmentId = 1,
                            JoiningDate = new DateTime(2019, 11, 20),
                            PhotoUrl = "/images/staff/irfan-cs.jpg",
                            IsActive = true
                        },
                        new()
                        {
                            Id = 5,
                            EmployeeCode = "EMP-GDCM-005",
                            FullName = "Asst. Prof. Zahir Shah",
                            Designation = "Assistant Professor",
                            BPSGrade = "BPS-18",
                            Qualification = "M.Phil English Literature & Modern Rhetoric",
                            DepartmentId = 3,
                            JoiningDate = new DateTime(2017, 2, 1),
                            PhotoUrl = "/images/staff/zahir-shah.jpg",
                            IsActive = true
                        },
                        new()
                        {
                            Id = 6,
                            EmployeeCode = "EMP-GDCM-006",
                            FullName = "Prof. Maulana Abdul Haq",
                            Designation = "Professor",
                            BPSGrade = "BPS-20",
                            Qualification = "Ph.D Islamic Jurisprudence & Shariah Sciences",
                            DepartmentId = 4,
                            JoiningDate = new DateTime(2008, 9, 1),
                            PhotoUrl = "/images/staff/abdul-haq.jpg",
                            IsActive = true
                        },
                        new()
                        {
                            Id = 7,
                            EmployeeCode = "EMP-GDCM-007",
                            FullName = "Assoc. Prof. Dr. Shaukat Khan",
                            Designation = "Associate Professor",
                            BPSGrade = "BPS-19",
                            Qualification = "Ph.D Botany (Medicinal Flora of Swat Valley)",
                            DepartmentId = 5,
                            JoiningDate = new DateTime(2014, 5, 12),
                            PhotoUrl = "/images/staff/shaukat-khan.jpg",
                            IsActive = true
                        },
                        new()
                        {
                            Id = 8,
                            EmployeeCode = "EMP-GDCM-008",
                            FullName = "Mr. Sher Muhammad Khan",
                            Designation = "Senior Clerk",
                            BPSGrade = "BPS-14",
                            Qualification = "B.Com & Office Automation Specialist",
                            DepartmentId = null,
                            JoiningDate = new DateTime(2011, 2, 1),
                            PhotoUrl = "/images/staff/sher-muhammad.jpg",
                            IsActive = true
                        }
                    };
                    await context.StaffProfiles.AddRangeAsync(staffList);
                    await context.SaveChangesAsync();
                }

                // 8. Seed 10 Realistic Enrolled Students
                if (!await context.StudentProfiles.AnyAsync())
                {
                    var students = new List<StudentProfile>
                    {
                        new()
                        {
                            Id = 1,
                            RollNumber = "GDCM-BSCS-26-001",
                            RegistrationNo = "UOS-2026-CS-0001",
                            FullName = "Muhammad Bilal Swati",
                            FatherName = "Fazal Wadood",
                            CNIC = "15602-1234567-1",
                            DateOfBirth = new DateTime(2005, 3, 14),
                            Phone = "0345-9876543",
                            DomicileDistrict = "Swat",
                            Address = "Main Bazaar, Madyan, District Swat, KP",
                            ProgramId = 4, // BSCS
                            CurrentSemester = 5,
                            Section = "A",
                            CGPA = 3.82m,
                            AdmissionDate = new DateTime(2024, 9, 1),
                            IsActive = true
                        },
                        new()
                        {
                            Id = 2,
                            RollNumber = "GDCM-BSCS-26-002",
                            RegistrationNo = "UOS-2026-CS-0002",
                            FullName = "Sohail Ahmad",
                            FatherName = "Said Karim",
                            CNIC = "15602-7654321-3",
                            DateOfBirth = new DateTime(2004, 7, 21),
                            Phone = "0301-8765432",
                            DomicileDistrict = "Swat",
                            Address = "Village Chail, Madyan, District Swat, KP",
                            ProgramId = 4, // BSCS
                            CurrentSemester = 5,
                            Section = "A",
                            CGPA = 3.65m,
                            AdmissionDate = new DateTime(2024, 9, 1),
                            IsActive = true
                        },
                        new()
                        {
                            Id = 3,
                            RollNumber = "GDCM-BSCS-26-003",
                            RegistrationNo = "UOS-2026-CS-0003",
                            FullName = "Hamza Ali Khan",
                            FatherName = "Bakht Rawan",
                            CNIC = "15602-9812345-5",
                            DateOfBirth = new DateTime(2005, 1, 10),
                            Phone = "0333-9123456",
                            DomicileDistrict = "Swat",
                            Address = "Near River Bridge, Bahrain, District Swat, KP",
                            ProgramId = 4, // BSCS
                            CurrentSemester = 5,
                            Section = "A",
                            CGPA = 3.45m,
                            AdmissionDate = new DateTime(2024, 9, 1),
                            IsActive = true
                        },
                        new()
                        {
                            Id = 4,
                            RollNumber = "GDCM-BSCS-26-004",
                            RegistrationNo = "UOS-2026-CS-0004",
                            FullName = "Tariq Aziz",
                            FatherName = "Aziz ur Rehman",
                            CNIC = "15602-4455667-9",
                            DateOfBirth = new DateTime(2005, 5, 18),
                            Phone = "0342-9988776",
                            DomicileDistrict = "Swat",
                            Address = "Miandam Road, Swat, KP",
                            ProgramId = 4, // BSCS
                            CurrentSemester = 5,
                            Section = "A",
                            CGPA = 3.20m,
                            AdmissionDate = new DateTime(2024, 9, 1),
                            IsActive = true
                        },
                        new()
                        {
                            Id = 5,
                            RollNumber = "GDCM-PM-26-001",
                            RegistrationNo = "BISE-SWAT-2026-101",
                            FullName = "Jawad Khan",
                            FatherName = "Khan Bahadar",
                            CNIC = "15602-1122334-7",
                            DateOfBirth = new DateTime(2008, 9, 12),
                            Phone = "0347-1122334",
                            DomicileDistrict = "Swat",
                            Address = "Fatehpur, Swat, KP",
                            ProgramId = 1, // F.Sc Pre-Med
                            CurrentSemester = 1,
                            Section = "A",
                            CGPA = 3.90m,
                            AdmissionDate = new DateTime(2026, 9, 1),
                            IsActive = true
                        },
                        new()
                        {
                            Id = 6,
                            RollNumber = "GDCM-PE-26-001",
                            RegistrationNo = "BISE-SWAT-2026-202",
                            FullName = "Usman Ghani",
                            FatherName = "Abdul Qayum",
                            CNIC = "15602-3344556-9",
                            DateOfBirth = new DateTime(2008, 4, 12),
                            Phone = "0346-1234567",
                            DomicileDistrict = "Swat",
                            Address = "Ashuran, Madyan, District Swat",
                            ProgramId = 2, // F.Sc Pre-Eng
                            CurrentSemester = 1,
                            Section = "A",
                            CGPA = 3.75m,
                            AdmissionDate = new DateTime(2026, 9, 1),
                            IsActive = true
                        },
                        new()
                        {
                            Id = 7,
                            RollNumber = "GDCM-ICS-26-001",
                            RegistrationNo = "BISE-SWAT-2026-303",
                            FullName = "Salman Faris",
                            FatherName = "Faris Khan",
                            CNIC = "15602-9988776-1",
                            DateOfBirth = new DateTime(2008, 6, 20),
                            Phone = "0314-5544332",
                            DomicileDistrict = "Swat",
                            Address = "Kalam Road, Madyan, Swat",
                            ProgramId = 3, // FCS/ICS
                            CurrentSemester = 1,
                            Section = "A",
                            CGPA = 3.50m,
                            AdmissionDate = new DateTime(2026, 9, 1),
                            IsActive = true
                        },
                        new()
                        {
                            Id = 8,
                            RollNumber = "GDCM-POL-26-001",
                            RegistrationNo = "UOS-2026-POL-001",
                            FullName = "Aftab Ahmad",
                            FatherName = "Gul Zada",
                            CNIC = "15602-5544332-7",
                            DateOfBirth = new DateTime(2005, 11, 5),
                            Phone = "0312-3456789",
                            DomicileDistrict = "Swat",
                            Address = "Madyan, District Swat, KP",
                            ProgramId = 5, // BS Pol Sci
                            CurrentSemester = 3,
                            Section = "A",
                            CGPA = 3.42m,
                            AdmissionDate = new DateTime(2025, 9, 1),
                            IsActive = true
                        },
                        new()
                        {
                            Id = 9,
                            RollNumber = "GDCM-ENG-26-001",
                            RegistrationNo = "UOS-2026-ENG-001",
                            FullName = "Naveed Ur Rahman",
                            FatherName = "Rahman Said",
                            CNIC = "15602-8877665-3",
                            DateOfBirth = new DateTime(2006, 2, 28),
                            Phone = "0335-7766554",
                            DomicileDistrict = "Swat",
                            Address = "Shin, Bahrain, District Swat",
                            ProgramId = 6, // BS English
                            CurrentSemester = 3,
                            Section = "A",
                            CGPA = 3.60m,
                            AdmissionDate = new DateTime(2025, 9, 1),
                            IsActive = true
                        },
                        new()
                        {
                            Id = 10,
                            RollNumber = "GDCM-BSCS-26-010",
                            RegistrationNo = "UOS-2026-CS-0010",
                            FullName = "Abid Ullah",
                            FatherName = "Sher Ali",
                            CNIC = "15602-2233445-5",
                            DateOfBirth = new DateTime(2005, 8, 14),
                            Phone = "0348-6655443",
                            DomicileDistrict = "Swat",
                            Address = "Upper Bazaar, Madyan, Swat",
                            ProgramId = 4, // BSCS
                            CurrentSemester = 5,
                            Section = "A",
                            CGPA = 3.15m,
                            AdmissionDate = new DateTime(2024, 9, 1),
                            IsActive = true
                        }
                    };
                    await context.StudentProfiles.AddRangeAsync(students);
                    await context.SaveChangesAsync();
                }

                // 9. Seed Sample Attendance Records (BS 4-Year Semester 5 & HSSC)
                if (!await context.AttendanceRecords.AnyAsync())
                {
                    var attendanceList = new List<AttendanceRecord>();
                    var baseDate = DateTime.UtcNow.Date;

                    // Seed 20 historical lecture sessions for BSCS Semester 5 Courses (6: DB, 7: OS, 8: Web)
                    // Demonstrating: Eligible (>=75%), Condonation Zone (65-74.9%), and Strictly Debarred (<65%)
                    for (int lecture = 1; lecture <= 20; lecture++)
                    {
                        var date = baseDate.AddDays(-(21 - lecture));

                        // Course 6: Database Systems (Instructor: Mr. Irfan Ahmad - StaffId 4)
                        // Student 1 (Ahmad Ali): 18 / 20 present = 90.0% (Eligible)
                        attendanceList.Add(new AttendanceRecord { StudentId = 1, CourseId = 6, Date = date, Status = (lecture == 3 || lecture == 11 ? "Absent" : "Present"), MarkedByStaffId = 4 });
                        // Student 2 (Bilal Ahmad): 12 / 20 present = 60.0% (Strictly Debarred <65%)
                        attendanceList.Add(new AttendanceRecord { StudentId = 2, CourseId = 6, Date = date, Status = (lecture % 3 == 0 || lecture == 7 || lecture == 13 ? "Absent" : "Present"), MarkedByStaffId = 4 });
                        // Student 3 (Hamza Khan): 14 / 20 present = 70.0% (Condonation Zone 65-74.9%)
                        attendanceList.Add(new AttendanceRecord { StudentId = 3, CourseId = 6, Date = date, Status = (lecture == 2 || lecture == 5 || lecture == 8 || lecture == 12 || lecture == 16 || lecture == 19 ? "Absent" : "Present"), MarkedByStaffId = 4 });
                        // Student 4 (Usman Tariq): 19 / 20 present = 95.0% (Eligible)
                        attendanceList.Add(new AttendanceRecord { StudentId = 4, CourseId = 6, Date = date, Status = (lecture == 14 ? "Absent" : "Present"), MarkedByStaffId = 4 });

                        // Course 7: Operating Systems (Instructor: Dr. Tariq - StaffId 2)
                        // Student 1: 17 / 20 present = 85.0% (Eligible)
                        attendanceList.Add(new AttendanceRecord { StudentId = 1, CourseId = 7, Date = date, Status = (lecture == 4 || lecture == 9 || lecture == 15 ? "Absent" : "Present"), MarkedByStaffId = 2 });
                        // Student 2: 19 / 20 present = 95.0% (Eligible)
                        attendanceList.Add(new AttendanceRecord { StudentId = 2, CourseId = 7, Date = date, Status = (lecture == 10 ? "Absent" : "Present"), MarkedByStaffId = 2 });
                        // Student 3: 16 / 20 present = 80.0% (Eligible)
                        attendanceList.Add(new AttendanceRecord { StudentId = 3, CourseId = 7, Date = date, Status = (lecture == 1 || lecture == 7 || lecture == 13 || lecture == 18 ? "Absent" : "Present"), MarkedByStaffId = 2 });
                        // Student 4: 18 / 20 present = 90.0% (Eligible)
                        attendanceList.Add(new AttendanceRecord { StudentId = 4, CourseId = 7, Date = date, Status = (lecture == 6 || lecture == 12 ? "Absent" : "Present"), MarkedByStaffId = 2 });

                        // Course 8: Web Technologies (Instructor: Mr. Irfan Ahmad - StaffId 4)
                        // Student 1: 14 / 20 present = 70.0% (Condonation Warning - Needs 4 classes to reach 75%)
                        attendanceList.Add(new AttendanceRecord { StudentId = 1, CourseId = 8, Date = date, Status = (lecture == 2 || lecture == 5 || lecture == 9 || lecture == 12 || lecture == 15 || lecture == 18 ? "Absent" : "Present"), MarkedByStaffId = 4 });
                        // Student 2: 16 / 20 present = 80.0% (Eligible)
                        attendanceList.Add(new AttendanceRecord { StudentId = 2, CourseId = 8, Date = date, Status = (lecture == 3 || lecture == 8 || lecture == 14 || lecture == 19 ? "Absent" : "Present"), MarkedByStaffId = 4 });
                        // Student 3: 15 / 20 present = 75.0% (Eligible - exact threshold)
                        attendanceList.Add(new AttendanceRecord { StudentId = 3, CourseId = 8, Date = date, Status = (lecture == 1 || lecture == 6 || lecture == 10 || lecture == 15 || lecture == 20 ? "Absent" : "Present"), MarkedByStaffId = 4 });
                        // Student 4: 17 / 20 present = 85.0% (Eligible)
                        attendanceList.Add(new AttendanceRecord { StudentId = 4, CourseId = 8, Date = date, Status = (lecture == 4 || lecture == 11 || lecture == 17 ? "Absent" : "Present"), MarkedByStaffId = 4 });
                    }

                    await context.AttendanceRecords.AddRangeAsync(attendanceList);
                    await context.SaveChangesAsync();
                }

                // 9b. Seed Approved Attendance Condonation (Student 3 in Course 6)
                if (!await context.AttendanceCondonations.AnyAsync())
                {
                    await context.AttendanceCondonations.AddAsync(new AttendanceCondonation
                    {
                        StudentId = 3,
                        CourseId = 6,
                        Semester = 5,
                        ActualAttendancePercentage = 70.00m,
                        ReasonAndMedicalProof = "DHQ Hospital Swat verified medical certificate for acute viral infection with bed rest. Approved by Academic Committee.",
                        FineChallanNumber = "BOK-COND-2026-001",
                        IsApprovedByPrincipal = true,
                        ApprovedByUserId = "principal@gdcmadyan.edu.pk",
                        ApprovedAt = DateTime.UtcNow.AddDays(-2),
                        CreatedAt = DateTime.UtcNow.AddDays(-5)
                    });
                    await context.SaveChangesAsync();
                }

                // 10. Seed Sample Exam Results & Grades (BSCS Semester 5 courses)
                if (!await context.ExamResults.AnyAsync())
                {
                    var examResults = new List<ExamResult>
                    {
                        new() { StudentId = 1, CourseId = 6, Semester = 5, MidTermMarks = 27, SessionalMarks = 18, FinalMarks = 45, TotalMarks = 90, Grade = "A", GradePoints = 4.00m, AttendancePercentageAtExam = 90.00m, IsDebarredForShortAttendance = false },
                        new() { StudentId = 1, CourseId = 7, Semester = 5, MidTermMarks = 25, SessionalMarks = 17, FinalMarks = 43, TotalMarks = 85, Grade = "A", GradePoints = 4.00m, AttendancePercentageAtExam = 85.00m, IsDebarredForShortAttendance = false },
                        new() { StudentId = 1, CourseId = 8, Semester = 5, MidTermMarks = 24, SessionalMarks = 16, FinalMarks = 42, TotalMarks = 82, Grade = "A-", GradePoints = 3.67m, AttendancePercentageAtExam = 70.00m, IsDebarredForShortAttendance = false },

                        // Student 2 is debarred in Course 6 (60.0% < 65%): FinalMarks locked to 0, Grade = SA (F)
                        new() { StudentId = 2, CourseId = 6, Semester = 5, MidTermMarks = 20, SessionalMarks = 14, FinalMarks = 0, TotalMarks = 34, Grade = "SA (F)", GradePoints = 0.00m, AttendancePercentageAtExam = 60.00m, IsDebarredForShortAttendance = true },
                        new() { StudentId = 2, CourseId = 7, Semester = 5, MidTermMarks = 22, SessionalMarks = 15, FinalMarks = 39, TotalMarks = 76, Grade = "B+", GradePoints = 3.33m, AttendancePercentageAtExam = 95.00m, IsDebarredForShortAttendance = false },
                        new() { StudentId = 2, CourseId = 8, Semester = 5, MidTermMarks = 21, SessionalMarks = 14, FinalMarks = 37, TotalMarks = 72, Grade = "B", GradePoints = 3.00m, AttendancePercentageAtExam = 80.00m, IsDebarredForShortAttendance = false },

                        // Student 3 has approved condonation in Course 6
                        new() { StudentId = 3, CourseId = 6, Semester = 5, MidTermMarks = 21, SessionalMarks = 14, FinalMarks = 38, TotalMarks = 73, Grade = "B", GradePoints = 3.00m, AttendancePercentageAtExam = 70.00m, IsDebarredForShortAttendance = false },
                        new() { StudentId = 4, CourseId = 6, Semester = 5, MidTermMarks = 19, SessionalMarks = 13, FinalMarks = 34, TotalMarks = 66, Grade = "C+", GradePoints = 2.33m, AttendancePercentageAtExam = 95.00m, IsDebarredForShortAttendance = false }
                    };
                    await context.ExamResults.AddRangeAsync(examResults);
                    await context.SaveChangesAsync();
                }

                // 11. Seed Sample Fee Challans (Paid & Unpaid BoK Vouchers)
                if (!await context.FeeChallans.AnyAsync())
                {
                    var challans = new List<FeeChallan>
                    {
                        new()
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
                            IssueDate = DateTime.UtcNow.AddDays(-20),
                            DueDate = DateTime.UtcNow.AddDays(5),
                            IsPaid = true,
                            PaidDate = DateTime.UtcNow.AddDays(-12),
                            BankReference = "BOK-MDY-DEP-8841"
                        },
                        new()
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
                            IssueDate = DateTime.UtcNow.AddDays(-20),
                            DueDate = DateTime.UtcNow.AddDays(5),
                            IsPaid = false,
                            PaidDate = null,
                            BankReference = null
                        },
                        new()
                        {
                            Id = 3,
                            ChallanNumber = "BOK-GDCM-2026-1003",
                            StudentId = 5,
                            Semester = 1,
                            TuitionFee = 3500.00m,
                            AdmissionFee = 1500.00m,
                            ExamFee = 1000.00m,
                            LibraryFund = 500.00m,
                            LabFund = 500.00m,
                            LateFine = 0.00m,
                            TotalAmount = 7000.00m,
                            IssueDate = DateTime.UtcNow.AddDays(-15),
                            DueDate = DateTime.UtcNow.AddDays(10),
                            IsPaid = true,
                            PaidDate = DateTime.UtcNow.AddDays(-8),
                            BankReference = "BOK-MDY-DEP-9104"
                        },
                        new()
                        {
                            Id = 4,
                            ChallanNumber = "BOK-GDCM-2026-1004",
                            StudentId = 6,
                            Semester = 1,
                            TuitionFee = 3500.00m,
                            AdmissionFee = 1500.00m,
                            ExamFee = 1000.00m,
                            LibraryFund = 500.00m,
                            LabFund = 500.00m,
                            LateFine = 0.00m,
                            TotalAmount = 7000.00m,
                            IssueDate = DateTime.UtcNow.AddDays(-15),
                            DueDate = DateTime.UtcNow.AddDays(10),
                            IsPaid = false,
                            PaidDate = null,
                            BankReference = null
                        }
                    };
                    await context.FeeChallans.AddRangeAsync(challans);
                    await context.SaveChangesAsync();
                }

                // 12. Seed ASP.NET Core Identity Users (With Exact Credentials Requested)
                
                // User 1: SuperAdmin / Principal
                var adminEmail = "admin@gdcmadyan.edu.pk";
                var adminUser = await userManager.FindByEmailAsync(adminEmail);
                if (adminUser == null)
                {
                    adminUser = new IdentityUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true
                    };
                    var result = await userManager.CreateAsync(adminUser, "Admin@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRolesAsync(adminUser, ["SuperAdmin", "Principal", "ControllerExams"]);
                        var principalStaff = await context.StaffProfiles.FirstOrDefaultAsync(s => s.Id == 1);
                        if (principalStaff != null)
                        {
                            principalStaff.IdentityUserId = adminUser.Id;
                            await context.SaveChangesAsync();
                        }
                    }
                }

                // User 2: Admission / Accounts Clerk
                var clerkEmail = "clerk@gdcmadyan.edu.pk";
                var clerkUser = await userManager.FindByEmailAsync(clerkEmail);
                if (clerkUser == null)
                {
                    clerkUser = new IdentityUser
                    {
                        UserName = clerkEmail,
                        Email = clerkEmail,
                        EmailConfirmed = true
                    };
                    var result = await userManager.CreateAsync(clerkUser, "Clerk@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(clerkUser, "Clerk");
                        var clerkStaff = await context.StaffProfiles.FirstOrDefaultAsync(s => s.Id == 8);
                        if (clerkStaff != null)
                        {
                            clerkStaff.IdentityUserId = clerkUser.Id;
                            await context.SaveChangesAsync();
                        }
                    }
                }

                // User 3: CS Lecturer (Faculty / HOD)
                var facultyEmail = "faculty.cs@gdcmadyan.edu.pk";
                var facultyUser = await userManager.FindByEmailAsync(facultyEmail);
                if (facultyUser == null)
                {
                    facultyUser = new IdentityUser
                    {
                        UserName = facultyEmail,
                        Email = facultyEmail,
                        EmailConfirmed = true
                    };
                    var result = await userManager.CreateAsync(facultyUser, "Faculty@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRolesAsync(facultyUser, ["Faculty", "HOD"]);
                        var lecturerStaff = await context.StaffProfiles.FirstOrDefaultAsync(s => s.Id == 4);
                        if (lecturerStaff != null)
                        {
                            lecturerStaff.IdentityUserId = facultyUser.Id;
                            await context.SaveChangesAsync();
                        }
                    }
                }

                // User 4: Sample BSCS Student (GDCM-BSCS-26-001)
                var studentRoll = "GDCM-BSCS-26-001";
                var studentUser = await userManager.FindByNameAsync(studentRoll);
                if (studentUser == null)
                {
                    studentUser = new IdentityUser
                    {
                        UserName = studentRoll,
                        Email = "student@gdcmadyan.edu.pk",
                        EmailConfirmed = true
                    };
                    var result = await userManager.CreateAsync(studentUser, "Student@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(studentUser, "Student");
                        var studentProfile = await context.StudentProfiles.FirstOrDefaultAsync(s => s.RollNumber == studentRoll);
                        if (studentProfile != null)
                        {
                            studentProfile.IdentityUserId = studentUser.Id;
                            await context.SaveChangesAsync();
                        }
                    }
                }

                // User 5: Dedicated Controller of Examinations
                var controllerEmail = "controller.exams@gdcmadyan.edu.pk";
                var controllerUser = await userManager.FindByEmailAsync(controllerEmail);
                if (controllerUser == null)
                {
                    controllerUser = new IdentityUser
                    {
                        UserName = controllerEmail,
                        Email = controllerEmail,
                        EmailConfirmed = true
                    };
                    var result = await userManager.CreateAsync(controllerUser, "Controller@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRolesAsync(controllerUser, ["ControllerExams", "Faculty"]);
                        var tariqStaff = await context.StaffProfiles.FirstOrDefaultAsync(s => s.Id == 2);
                        if (tariqStaff != null && string.IsNullOrEmpty(tariqStaff.IdentityUserId))
                        {
                            tariqStaff.IdentityUserId = controllerUser.Id;
                            await context.SaveChangesAsync();
                        }
                    }
                }

                // User 6: Sample F.Sc Pre-Medical Student (GDCM-PM-26-001)
                var fscRoll = "GDCM-PM-26-001";
                var fscUser = await userManager.FindByNameAsync(fscRoll);
                if (fscUser == null)
                {
                    fscUser = new IdentityUser
                    {
                        UserName = fscRoll,
                        Email = "jawad.fsc@gdcmadyan.edu.pk",
                        EmailConfirmed = true
                    };
                    var result = await userManager.CreateAsync(fscUser, "Student@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(fscUser, "Student");
                        var fscStudentProfile = await context.StudentProfiles.FirstOrDefaultAsync(s => s.RollNumber == fscRoll);
                        if (fscStudentProfile != null)
                        {
                            fscStudentProfile.IdentityUserId = fscUser.Id;
                            await context.SaveChangesAsync();
                        }
                    }
                }

                // 13. Seed Teacher Course Assignments (Strict Subject-Level Security)
                if (!await context.TeacherCourseAssignments.AnyAsync())
                {
                    var assignments = new List<TeacherCourseAssignment>
                    {
                        new() { StaffId = 4, CourseId = 6, AcademicSession = "Fall 2026", Section = "A", IsActive = true }, // Mr. Irfan -> Database Systems (BSCS Sem 5)
                        new() { StaffId = 4, CourseId = 8, AcademicSession = "Fall 2026", Section = "A", IsActive = true }, // Mr. Irfan -> Web Technologies (BSCS Sem 5)
                        new() { StaffId = 2, CourseId = 7, AcademicSession = "Fall 2026", Section = "A", IsActive = true }, // Dr. Tariq -> Operating Systems (BSCS Sem 5)
                        new() { StaffId = 7, CourseId = 11, AcademicSession = "2026-2027", Section = "A", IsActive = true } // Dr. Shaukat -> Cell Biology (F.Sc Pre-Med Sem 1)
                    };
                    await context.TeacherCourseAssignments.AddRangeAsync(assignments);
                    await context.SaveChangesAsync();
                }

                // 14. Seed HSSC Attendance History & Sample Rusticated Student
                var fscStudent = await context.StudentProfiles.FirstOrDefaultAsync(s => s.Id == 5); // Jawad Khan (F.Sc Pre-Med)
                if (fscStudent != null)
                {
                    var today = DateTime.UtcNow.Date;
                    if (!await context.AttendanceRecords.AnyAsync(a => a.StudentId == fscStudent.Id && a.CourseId == 11))
                    {
                        await context.AttendanceRecords.AddRangeAsync(new List<AttendanceRecord>
                        {
                            new() { StudentId = fscStudent.Id, CourseId = 11, Date = today.AddDays(-2), Status = "Absent", MarkedByStaffId = 7 },
                            new() { StudentId = fscStudent.Id, CourseId = 11, Date = today.AddDays(-1), Status = "Absent", MarkedByStaffId = 7 }
                        });
                        fscStudent.ConsecutiveAbsentDays = 2;
                        await context.SaveChangesAsync();
                    }
                }

                // Seed one currently rusticated student for Parent Visit demonstration (Student 6: F.Sc Pre-Engineering)
                var rusticatedStudent = await context.StudentProfiles.FirstOrDefaultAsync(s => s.Id == 6);
                if (rusticatedStudent != null && rusticatedStudent.AcademicStatus != AcademicStatus.Rusticated_ParentRequired)
                {
                    rusticatedStudent.AcademicStatus = AcademicStatus.Rusticated_ParentRequired;
                    rusticatedStudent.ConsecutiveAbsentDays = 3;
                    if (!await context.RusticationRecords.AnyAsync(r => r.StudentId == rusticatedStudent.Id && !r.IsReinstated))
                    {
                        await context.RusticationRecords.AddAsync(new RusticationRecord
                        {
                            StudentId = rusticatedStudent.Id,
                            RusticationDate = DateTime.UtcNow.AddDays(-2),
                            AbsentDatesSummary = "Oct 26, Oct 27, Oct 28",
                            Reason = "Absent for 3 consecutive class days in HSSC without approved leave",
                            CourseId = 12,
                            IsReinstated = false
                        });
                    }
                    await context.SaveChangesAsync();
                }

                logger.LogInformation("GDC Madyan Swat institutional database and Identity roles seeded successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding GDC Madyan Swat database.");
                throw;
            }
        }
    }
}
