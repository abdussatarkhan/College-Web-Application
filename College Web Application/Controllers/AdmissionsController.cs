using College_Web_Application.Data;
using College_Web_Application.Models;
using College_Web_Application.Models.ViewModels;
using College_Web_Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace College_Web_Application.Controllers
{
    public class AdmissionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IMemoryCache _cache;
        private readonly IAdmissionSettingsService _settingsService;
        private readonly ILogger<AdmissionsController> _logger;

        private const string MeritCachePrefix = "GDC_MeritList_";

        public AdmissionsController(
            ApplicationDbContext context,
            IWebHostEnvironment environment,
            IMemoryCache cache,
            IAdmissionSettingsService settingsService,
            ILogger<AdmissionsController> logger)
        {
            _context = context;
            _environment = environment;
            _cache = cache;
            _settingsService = settingsService;
            _logger = logger;
        }

        // GET: /Admissions/Guide
        [HttpGet]
        public async Task<IActionResult> Guide()
        {
            var settings = await _settingsService.GetSettingsAsync();
            ViewBag.AdmissionSettings = settings;

            var programs = await _context.AcademicPrograms
                .AsNoTracking()
                .Include(p => p.Department)
                .OrderBy(p => p.Level)
                .ThenBy(p => p.Title)
                .ToListAsync();

            return View(programs);
        }

        // GET: /Admissions/Apply
        [HttpGet]
        public async Task<IActionResult> Apply()
        {
            var settings = await _settingsService.GetSettingsAsync();
            if (!settings.IsBsAdmissionsOpen && !settings.IsFscAdmissionsOpen)
            {
                return View("Closed", settings);
            }

            var programsQuery = _context.AcademicPrograms.AsNoTracking().AsQueryable();

            if (!settings.IsBsAdmissionsOpen)
            {
                programsQuery = programsQuery.Where(p => p.Level == "HSSC" || p.ProgramLevel == ProgramLevel.HSSC_FSc);
            }

            if (!settings.IsFscAdmissionsOpen)
            {
                programsQuery = programsQuery.Where(p => p.Level != "HSSC" && p.ProgramLevel != ProgramLevel.HSSC_FSc);
            }

            var programs = await programsQuery
                .OrderBy(p => p.Level)
                .ThenBy(p => p.Title)
                .ToListAsync();

            var viewModel = new AdmissionApplyViewModel
            {
                Programs = programs
            };

            ViewBag.AdmissionSettings = settings;
            return View(viewModel);
        }

        // POST: /Admissions/Apply
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(AdmissionApplyViewModel model)
        {
            var settings = await _settingsService.GetSettingsAsync();
            ViewBag.AdmissionSettings = settings;

            if (!settings.IsBsAdmissionsOpen && !settings.IsFscAdmissionsOpen)
            {
                return View("Closed", settings);
            }

            if (!ModelState.IsValid)
            {
                model.Programs = await GetOpenProgramsAsync(settings);
                return View(model);
            }

            var program = await _context.AcademicPrograms.FindAsync(model.AppliedProgramId);
            if (program == null)
            {
                ModelState.AddModelError(nameof(model.AppliedProgramId), "Invalid program selected.");
                model.Programs = await GetOpenProgramsAsync(settings);
                return View(model);
            }

            bool isProgramBs = program.Level == "BS" || program.ProgramLevel == ProgramLevel.Undergraduate_BS;
            bool isProgramFsc = program.Level == "HSSC" || program.ProgramLevel == ProgramLevel.HSSC_FSc;

            if (isProgramBs && !settings.IsBsAdmissionsOpen)
            {
                ModelState.AddModelError(string.Empty, "Online admissions for BS 4-Year degree programs are currently closed by order of the Principal's Office.");
                model.Programs = await GetOpenProgramsAsync(settings);
                return View(model);
            }

            if (isProgramFsc && !settings.IsFscAdmissionsOpen)
            {
                ModelState.AddModelError(string.Empty, "Online admissions for Intermediate (F.Sc / ICS) programs are currently closed by order of the Principal's Office.");
                model.Programs = await GetOpenProgramsAsync(settings);
                return View(model);
            }

            // Duplicate prevention for same CNIC and Program
            bool alreadyApplied = await _context.AdmissionApplications
                .AnyAsync(a => a.CNIC == model.CNIC && a.AppliedProgramId == model.AppliedProgramId);

            if (alreadyApplied)
            {
                ModelState.AddModelError(string.Empty, $"An application with CNIC {model.CNIC} for {program.Title} already exists. You can track your existing application using Track Status.");
                model.Programs = await _context.AcademicPrograms.AsNoTracking().ToListAsync();
                return View(model);
            }

            // =========================================================================
            // KP HED & BISE SWAT MARKS-BASED MERIT CALCULATION ENGINE
            // In Khyber Pakhtunkhwa Government Colleges, merit is ranked strictly by
            // Total Obtained Marks (+ 20 bonus marks for verified Hafiz-e-Quran).
            // =========================================================================
            decimal hafizMarks = model.IsHafizQuran ? 20.00m : 0.00m;
            decimal calculatedMeritMarks = 0.00m;

            if (program.Level == "HSSC")
            {
                // Intermediate Merit = Matric Obtained Marks + Hafiz Bonus (out of 1100)
                calculatedMeritMarks = model.SscObtainedMarks + hafizMarks;
            }
            else // BS 4-Year Undergraduate
            {
                // Undergraduate BS Merit under University of Swat regulations:
                // Primary merit score is determined by HSSC Obtained Marks + Hafiz Bonus (out of 1100)
                decimal baseObtained = (model.HsscObtainedMarks.HasValue && model.HsscObtainedMarks.Value > 0)
                    ? model.HsscObtainedMarks.Value
                    : model.SscObtainedMarks;

                calculatedMeritMarks = baseObtained + hafizMarks;
            }

            // Generate unique human-readable tracking number: GDCM-ADM-2026-0001
            int count = await _context.AdmissionApplications.CountAsync() + 1;
            string trackingNo = $"GDCM-ADM-{DateTime.UtcNow.Year}-{count:D4}";

            // Handle optional document uploads
            string? photoPath = await SaveFileAsync(model.PassportPhoto, "photos");
            string? dmcPath = await SaveFileAsync(model.DmcScanFile, "dmcs");
            string? domicilePath = await SaveFileAsync(model.DomicileOrBFormFile, "domiciles");

            var application = new AdmissionApplication
            {
                TrackingNumber = trackingNo,
                ApplicantName = model.ApplicantName.Trim(),
                FatherName = model.FatherName.Trim(),
                CNIC = model.CNIC.Trim(),
                Gender = model.Gender,
                BloodGroup = model.BloodGroup,
                DateOfBirth = model.DateOfBirth,
                Phone = model.Phone.Trim(),
                GuardianPhone = model.GuardianPhone?.Trim(),
                DomicileDistrict = model.DomicileDistrict.Trim(),
                Tehsil = model.Tehsil.Trim(),
                PostalAddress = model.PostalAddress.Trim(),
                SscBoard = model.SscBoard,
                SscRollNumber = model.SscRollNumber.Trim(),
                SscPassingYear = model.SscPassingYear,
                MatricObtainedMarks = model.SscObtainedMarks,
                MatricTotalMarks = model.SscTotalMarks,
                HsscBoard = model.HsscBoard,
                HsscRollNumber = model.HsscRollNumber?.Trim(),
                HsscPassingYear = model.HsscPassingYear,
                HsscObtainedMarks = model.HsscObtainedMarks,
                HsscTotalMarks = model.HsscTotalMarks,
                IsHafizQuran = model.IsHafizQuran,
                HafizMarks = hafizMarks,
                MeritScore = calculatedMeritMarks, // Stored as absolute Marks
                AppliedProgramId = model.AppliedProgramId,
                SecondChoiceProgramId = model.SecondChoiceProgramId,
                QuotaCategory = model.QuotaCategory,
                PassportPhotoPath = photoPath,
                DmcScanPath = dmcPath,
                DomicileOrBFormPath = domicilePath,
                Status = "Pending",
                AppliedDate = DateTime.UtcNow
            };

            await _context.AdmissionApplications.AddAsync(application);
            await _context.SaveChangesAsync();

            _logger.LogInformation("New admission application received: TrackingNumber={TrackingNumber}, Name={Name}, Program={Program}, Marks={Marks}",
                trackingNo, application.ApplicantName, program.Title, calculatedMeritMarks);

            // Invalidate merit list cache
            // Cache keys will expire naturally or on next page load

            TempData["SuccessTrackingNo"] = trackingNo;
            TempData["ApplicantName"] = application.ApplicantName;
            TempData["ProgramTitle"] = program.Title;
            TempData["MeritMarks"] = calculatedMeritMarks.ToString("0.00");

            return RedirectToAction(nameof(Confirmation), new { tracking = trackingNo });
        }

        // GET: /Admissions/Confirmation
        public async Task<IActionResult> Confirmation(string tracking)
        {
            if (string.IsNullOrWhiteSpace(tracking))
            {
                return RedirectToAction(nameof(Apply));
            }

            var app = await _context.AdmissionApplications
                .AsNoTracking()
                .Include(a => a.AppliedProgram)
                .FirstOrDefaultAsync(a => a.TrackingNumber == tracking);

            if (app == null)
            {
                return NotFound("Application not found.");
            }

            return View(app);
        }

        // GET: /Admissions/TrackStatus
        public async Task<IActionResult> TrackStatus(string? query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return View((ApplicationStatusViewModel?)null);
            }

            var q = query.Trim();
            var app = await _context.AdmissionApplications
                .AsNoTracking()
                .Include(a => a.AppliedProgram)
                .FirstOrDefaultAsync(a => a.TrackingNumber.ToLower() == q.ToLower() || a.CNIC == q);

            if (app == null)
            {
                ViewBag.NotFoundMessage = $"No admission application found matching '{query}'. Please verify your Tracking Number or CNIC format (15602-xxxxxxx-x).";
                return View((ApplicationStatusViewModel?)null);
            }

            var vm = new ApplicationStatusViewModel
            {
                TrackingNumber = app.TrackingNumber,
                ApplicantName = app.ApplicantName,
                FatherName = app.FatherName,
                CNIC = app.CNIC,
                AppliedProgramTitle = app.AppliedProgram?.Title ?? "Program",
                QuotaCategory = app.QuotaCategory,
                MeritScore = app.MeritScore,
                Status = app.Status,
                AppliedDate = app.AppliedDate,
                MatricObtainedMarks = app.MatricObtainedMarks,
                MatricTotalMarks = app.MatricTotalMarks,
                HsscObtainedMarks = app.HsscObtainedMarks,
                HsscTotalMarks = app.HsscTotalMarks,
                IsHafizQuran = app.IsHafizQuran
            };

            return View(vm);
        }

        // GET: /Admissions/MeritLists
        public async Task<IActionResult> MeritLists(int? programId, string quota = "All", string? search = null, int page = 1)
        {
            const int pageSize = 50;
            if (page < 1) page = 1;

            string cacheKey = $"{MeritCachePrefix}{programId ?? 0}_{quota}_{search ?? "none"}_{page}";

            if (!_cache.TryGetValue(cacheKey, out MeritListFilterViewModel? viewModel) || viewModel == null)
            {
                var query = _context.AdmissionApplications
                    .AsNoTracking()
                    .Include(a => a.AppliedProgram)
                    .AsQueryable();

                if (programId.HasValue && programId.Value > 0)
                {
                    query = query.Where(a => a.AppliedProgramId == programId.Value);
                }

                if (!string.IsNullOrWhiteSpace(quota) && quota != "All")
                {
                    query = query.Where(a => a.QuotaCategory == quota);
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var keyword = search.Trim().ToLower();
                    query = query.Where(a =>
                        a.ApplicantName.ToLower().Contains(keyword) ||
                        a.FatherName.ToLower().Contains(keyword) ||
                        a.TrackingNumber.ToLower().Contains(keyword) ||
                        a.CNIC.Contains(keyword));
                }

                int totalCount = await query.CountAsync();

                // Sort strictly by Total Merit Marks descending!
                var apps = await query
                    .OrderByDescending(a => a.MeritScore)
                    .ThenBy(a => a.AppliedDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                decimal cutOffMarks = apps.Any() ? apps.Min(a => a.MeritScore) : 0.00m;
                decimal highestMarks = apps.Any() ? apps.Max(a => a.MeritScore) : 0.00m;
                decimal cutOffPct = 0.00m;

                var sampleApp = apps.FirstOrDefault();
                if (sampleApp != null)
                {
                    decimal totalDenominator = (sampleApp.HsscTotalMarks.HasValue && sampleApp.HsscTotalMarks.Value > 0) 
                        ? sampleApp.HsscTotalMarks.Value 
                        : (sampleApp.MatricTotalMarks > 0 ? sampleApp.MatricTotalMarks : 1100.00m);
                    cutOffPct = Math.Round((cutOffMarks / totalDenominator) * 100m, 2);
                }

                viewModel = new MeritListFilterViewModel
                {
                    Applications = apps,
                    ProgramId = programId,
                    QuotaCategory = quota,
                    SearchQuery = search,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    CutOffMeritMarks = cutOffMarks,
                    HighestMeritMarks = highestMarks,
                    CutOffMeritPercentage = cutOffPct,
                    Programs = await _context.AcademicPrograms.AsNoTracking().OrderBy(p => p.Title).ToListAsync()
                };

                // Cache for 10 minutes to protect against 4G traffic spikes
                _cache.Set(cacheKey, viewModel, TimeSpan.FromMinutes(10));
            }

            return View(viewModel);
        }

        private async Task<string?> SaveFileAsync(IFormFile? file, string subfolder)
        {
            if (file == null || file.Length == 0) return null;

            var ext = Path.GetExtension(file.FileName).ToLower();
            string[] allowed = [".jpg", ".jpeg", ".png", ".pdf"];
            if (!allowed.Contains(ext)) return null;

            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "admissions", subfolder);
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            var uniqueName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(uploadsPath, uniqueName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/admissions/{subfolder}/{uniqueName}";
        }

        private async Task<List<AcademicProgram>> GetOpenProgramsAsync(AdmissionSettings settings)
        {
            var query = _context.AcademicPrograms.AsNoTracking().AsQueryable();

            if (!settings.IsBsAdmissionsOpen)
            {
                query = query.Where(p => p.Level == "HSSC" || p.ProgramLevel == ProgramLevel.HSSC_FSc);
            }

            if (!settings.IsFscAdmissionsOpen)
            {
                query = query.Where(p => p.Level != "HSSC" && p.ProgramLevel != ProgramLevel.HSSC_FSc);
            }

            return await query.OrderBy(p => p.Level).ThenBy(p => p.Title).ToListAsync();
        }
    }
}
