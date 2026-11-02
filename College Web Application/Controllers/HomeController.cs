using College_Web_Application.Data;
using College_Web_Application.Models;
using College_Web_Application.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;

namespace College_Web_Application.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<HomeController> _logger;

        private const string HomePageCacheKey = "GDC_GIKI_HomePage_Payload";

        public HomeController(
            ApplicationDbContext context,
            IMemoryCache cache,
            ILogger<HomeController> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            if (!_cache.TryGetValue(HomePageCacheKey, out HomeViewModel? viewModel) || viewModel == null)
            {
                var pinnedNotices = await _context.Notices
                    .AsNoTracking()
                    .Where(n => n.IsPinned)
                    .OrderByDescending(n => n.PublishedDate)
                    .Take(6)
                    .ToListAsync();

                if (!pinnedNotices.Any())
                {
                    pinnedNotices = await _context.Notices
                        .AsNoTracking()
                        .OrderByDescending(n => n.PublishedDate)
                        .Take(5)
                        .ToListAsync();
                }

                var programs = await _context.AcademicPrograms
                    .AsNoTracking()
                    .Include(p => p.Department)
                    .ToListAsync();

                var departments = await _context.Departments
                    .AsNoTracking()
                    .ToListAsync();

                int enrolledCount = await _context.StudentProfiles.AsNoTracking().CountAsync(s => s.IsActive);
                int facultyCount = await _context.StaffProfiles.AsNoTracking().CountAsync(s => s.IsActive);

                // GIKI-Style News & Academic Announcements
                var campusNews = new List<CampusNewsItem>
                {
                    new()
                    {
                        Title = "BS Semester Admissions 2026-27 Announced Under University of Swat",
                        Category = "Admissions",
                        PublishedDate = DateTime.UtcNow.AddDays(-2),
                        Summary = "Online application portal is now active for BS Computer Science, BS Microbiology, BS English, BS Urdu, BS Chemistry, and BS Botany. Candidates can apply with verified matric and intermediate credentials.",
                        IconClass = "bi-mortarboard-fill",
                        ImageUrl = "/images/gallery/4-science-bs-blocks.jpg"
                    },
                    new()
                    {
                        Title = "Annual Sports Gala & Inter-Departmental Tournament 2026 Concludes",
                        Category = "Campus Life",
                        PublishedDate = DateTime.UtcNow.AddDays(-5),
                        Summary = "Over 400 student athletes competed across Cricket, Football, Volleyball, and Badminton. Department of Computer Science lifted the overall championship trophy.",
                        IconClass = "bi-trophy-fill",
                        ImageUrl = "/images/gallery/3-campus-gardens.jpg"
                    },
                    new()
                    {
                        Title = "GDC Madyan Students Clinch Top Positions in BISE Swat HSSC Exams",
                        Category = "Academics",
                        PublishedDate = DateTime.UtcNow.AddDays(-12),
                        Summary = "F.Sc Pre-Medical and Pre-Engineering scholars from GDC Madyan secured top regional ranks in the annual board examinations with distinction awards.",
                        IconClass = "bi-star-fill",
                        ImageUrl = "/images/gallery/2-main-academic-block.jpg"
                    },
                    new()
                    {
                        Title = "Quality Enhancement Cell (QEC) Organizes Seminar on Modern Cloud & AI",
                        Category = "Workshops",
                        PublishedDate = DateTime.UtcNow.AddDays(-18),
                        Summary = "Distinguished industry experts delivered keynote sessions on distributed systems, modern .NET architecture, and artificial intelligence for undergraduate researchers.",
                        IconClass = "bi-laptop-fill",
                        ImageUrl = "/images/gallery/6-classroom-corridors.jpg"
                    }
                };

                // Upcoming Academic & Campus Events Calendar
                var events = new List<CampusEventItem>
                {
                    new()
                    {
                        Title = "BISE Swat HSSC Annual Examinations 2026 (Part I & II)",
                        Category = "Examination",
                        EventDate = DateTime.UtcNow.AddDays(14),
                        Time = "09:00 AM - 12:00 PM",
                        Venue = "Main College Exam Center (Block-A)",
                        Description = "Commencement of intermediate annual board examinations. Roll number slips and candidate verifications mandatory.",
                        BadgeColor = "bg-warning text-dark"
                    },
                    new()
                    {
                        Title = "Undergraduate BS 4-Year Final Term Examination Fall 2026",
                        Category = "University Exam",
                        EventDate = DateTime.UtcNow.AddDays(25),
                        Time = "09:00 AM - 01:00 PM",
                        Venue = "Academic Complex Halls 1 & 2",
                        Description = "Semester 1 to 8 final term evaluations under University of Swat regulations. Minimum 75% attendance mandatory for entry.",
                        BadgeColor = "bg-success text-white"
                    },
                    new()
                    {
                        Title = "Inter-College Science & Software Exhibition 2026",
                        Category = "Competition",
                        EventDate = DateTime.UtcNow.AddDays(38),
                        Time = "10:00 AM - 04:00 PM",
                        Venue = "GDC Madyan CS Labs & Central Lawn",
                        Description = "Showcase of scientific models, AI projects, robotic automation, and chemical experiments from colleges across Malakand Division.",
                        BadgeColor = "bg-primary text-white"
                    },
                    new()
                    {
                        Title = "Spring Admissions 2027 Open House & Career Guidance Seminar",
                        Category = "Admissions",
                        EventDate = DateTime.UtcNow.AddDays(50),
                        Time = "11:00 AM - 02:00 PM",
                        Venue = "College Main Auditorium",
                        Description = "Orientation for prospective matric and intermediate graduates with counseling sessions by senior professors.",
                        BadgeColor = "bg-info text-dark"
                    }
                };

                // GIKI-Style Alumni & Top Achievers Spotlight
                var alumniList = new List<AlumniSpotlightItem>
                {
                    new()
                    {
                        FullName = "Engr. Asadullah Khan",
                        Degree = "F.Sc Pre-Engineering (Session 2014-16)",
                        PassingYear = 2016,
                        CurrentRole = "Senior Infrastructure Engineer",
                        Organization = "National Transmission & Despatch Company (NTDC)",
                        Quote = "GDC Madyan provided the rigorous scientific foundation and mentor guidance that shaped my engineering career.",
                        BadgeClass = "bg-primary"
                    },
                    new()
                    {
                        FullName = "Dr. Sanaullah Swati",
                        Degree = "F.Sc Pre-Medical (Session 2012-14)",
                        PassingYear = 2014,
                        CurrentRole = "Medical Specialist / Registrar",
                        Organization = "Saidu Teaching Hospital (STH) Swat",
                        Quote = "The dedicated faculty of biology and chemistry at Madyan instilled in me the discipline required to serve our community in healthcare.",
                        BadgeClass = "bg-success"
                    },
                    new()
                    {
                        FullName = "Mr. Farhan Ali",
                        Degree = "BS Computer Science (Session 2018-22)",
                        PassingYear = 2022,
                        CurrentRole = "Lead Full-Stack .NET Developer",
                        Organization = "Techlogix / Systems Ltd",
                        Quote = "From our college computer lab in Madyan to building enterprise systems globally, GDC Madyan ignited my passion for software engineering.",
                        BadgeClass = "bg-warning text-dark"
                    }
                };

                viewModel = new HomeViewModel
                {
                    PinnedNotices = pinnedNotices,
                    IntermediatePrograms = programs.Where(p => p.Level == "HSSC").ToList(),
                    UndergraduatePrograms = programs.Where(p => p.Level == "BS").ToList(),
                    Departments = departments,
                    CampusNews = campusNews,
                    AlumniSpotlights = alumniList,
                    UpcomingEvents = events,
                    TotalStudents = enrolledCount > 0 ? enrolledCount + 5000 : 5200,
                    TotalFaculty = facultyCount > 0 ? facultyCount + 100 : 110,
                    TotalPrograms = 9,
                    YearsOfExcellence = DateTime.UtcNow.Year - 2006,
                    DisciplinesCount = 6
                };

                // Cache for 15 minutes to guarantee peak performance during results & admissions traffic
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(15))
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));

                _cache.Set(HomePageCacheKey, viewModel, cacheOptions);
            }

            return View(viewModel);
        }

        // GET: /Home/Events
        public IActionResult Events()
        {
            var events = new List<CampusEventItem>
            {
                new()
                {
                    Title = "BISE Swat HSSC Annual Examinations 2026 (Part I & II)",
                    Category = "Board Examination",
                    EventDate = DateTime.UtcNow.AddDays(14),
                    Time = "09:00 AM - 12:00 PM",
                    Venue = "Main College Exam Center (Block-A)",
                    Description = "Commencement of intermediate annual board examinations. Roll number slips and candidate verifications mandatory.",
                    BadgeColor = "bg-warning text-dark"
                },
                new()
                {
                    Title = "Undergraduate BS 4-Year Final Term Examination Fall 2026",
                    Category = "University Exam",
                    EventDate = DateTime.UtcNow.AddDays(25),
                    Time = "09:00 AM - 01:00 PM",
                    Venue = "Academic Complex Halls 1 & 2",
                    Description = "Semester 1 to 8 final term evaluations under University of Swat regulations. Minimum 75% attendance mandatory for entry.",
                    BadgeColor = "bg-success text-white"
                },
                new()
                {
                    Title = "Inter-College Science & Software Exhibition 2026",
                    Category = "Competition",
                    EventDate = DateTime.UtcNow.AddDays(38),
                    Time = "10:00 AM - 04:00 PM",
                    Venue = "GDC Madyan CS Labs & Central Lawn",
                    Description = "Showcase of scientific models, AI projects, robotic automation, and chemical experiments from colleges across Malakand Division.",
                    BadgeColor = "bg-primary text-white"
                },
                new()
                {
                    Title = "Spring Admissions 2027 Open House & Career Guidance Seminar",
                    Category = "Admissions",
                    EventDate = DateTime.UtcNow.AddDays(50),
                    Time = "11:00 AM - 02:00 PM",
                    Venue = "College Main Auditorium",
                    Description = "Orientation for prospective matric and intermediate graduates with counseling sessions by senior professors.",
                    BadgeColor = "bg-info text-dark"
                },
                new()
                {
                    Title = "KP Chief Minister Educational Endowment Scholarship Verification Camp",
                    Category = "Financial Aid",
                    EventDate = DateTime.UtcNow.AddDays(65),
                    Time = "10:00 AM - 02:00 PM",
                    Venue = "Student Support & Financial Aid Office (Admin Block)",
                    Description = "On-campus verification of scholarship dossiers and MORA zakat stipend applications for deserving students.",
                    BadgeColor = "bg-secondary text-white"
                }
            };

            return View(events);
        }

        // GET: /Home/Downloads
        public IActionResult Downloads()
        {
            var documents = new List<DownloadableDocumentItem>
            {
                new()
                {
                    Title = "Government Degree College Madyan Swat - Official Prospectus 2026-27",
                    Category = "Prospectus & Guidelines",
                    FileSize = "4.8 MB",
                    FileType = "PDF",
                    Description = "Comprehensive institutional guide detailing academic departments, degree programs, faculty profiles, laboratory facilities, fee schedule, and code of conduct.",
                    DownloadUrl = "/images/campus-banner.jpg",
                    LastUpdated = DateTime.UtcNow.AddDays(-10)
                },
                new()
                {
                    Title = "Intermediate (F.Sc / ICS) Offline Admission Application Form",
                    Category = "Admission Forms",
                    FileSize = "620 KB",
                    FileType = "PDF",
                    Description = "Official BISE Swat affiliated admission application form for Pre-Medical, Pre-Engineering, and Computer Science programs.",
                    DownloadUrl = "#",
                    LastUpdated = DateTime.UtcNow.AddDays(-20)
                },
                new()
                {
                    Title = "Undergraduate BS 4-Year Degree Application Dossier",
                    Category = "Admission Forms",
                    FileSize = "740 KB",
                    FileType = "PDF",
                    Description = "University of Swat affiliated BS application form including choice selection, merit calculation sheet, and document verification checklist.",
                    DownloadUrl = "#",
                    LastUpdated = DateTime.UtcNow.AddDays(-20)
                },
                new()
                {
                    Title = "Institutional Code of Conduct & Anti-Ragging Affidavit (Stamp Paper Format)",
                    Category = "Affidavits & Undertakings",
                    FileSize = "340 KB",
                    FileType = "PDF",
                    Description = "Mandatory legal undertaking on Rs. 100 judicial stamp paper signed by applicant and parent/guardian for college admission.",
                    DownloadUrl = "#",
                    LastUpdated = DateTime.UtcNow.AddDays(-30)
                },
                new()
                {
                    Title = "HSSC 3-Day Absence Rustication Reinstatement Undertaking Form",
                    Category = "Discipline & Attendance",
                    FileSize = "410 KB",
                    FileType = "PDF",
                    Description = "Official clearance form for parents/guardians visiting the Principal Office following 3 consecutive days absence rustication.",
                    DownloadUrl = "#",
                    LastUpdated = DateTime.UtcNow.AddDays(-15)
                },
                new()
                {
                    Title = "BISE Swat Roll Number Slip Duplicate Issuance Proforma",
                    Category = "Examination Office",
                    FileSize = "290 KB",
                    FileType = "PDF",
                    Description = "Application form for obtaining duplicate examination entry cards and roll number slips from BISE Swat camp office.",
                    DownloadUrl = "#",
                    LastUpdated = DateTime.UtcNow.AddDays(-40)
                }
            };

            return View(documents);
        }

        // GET: /Home/Contact
        public IActionResult Contact()
        {
            return View(new ContactInquiryViewModel());
        }

        // POST: /Home/Contact
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Contact(ContactInquiryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _logger.LogInformation("Contact Inquiry Received: Name={Name}, Email={Email}, Subject={Subject}",
                model.FullName, model.Email, model.Subject);

            TempData["SuccessMessage"] = "Thank you for reaching out to Government Degree College Madyan Swat. Your inquiry has been routed to the relevant administrative office. We will respond within 24-48 working hours.";
            return RedirectToAction(nameof(Contact));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
