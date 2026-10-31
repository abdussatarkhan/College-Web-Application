using College_Web_Application.Data;
using College_Web_Application.Models;
using College_Web_Application.Models.ViewModels;
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
        private readonly ILogger<AdmissionsController> _logger;

        private const string MeritCachePrefix = "GDC_MeritList_";

        public AdmissionsController(
            ApplicationDbContext context,
            IWebHostEnvironment environment,
            IMemoryCache cache,
            ILogger<AdmissionsController> logger)
        {
            _context = context;
            _environment = environment;
            _cache = cache;
            _logger = logger;
        }

        // GET: /Admissions/Apply
        [HttpGet]
        public async Task<IActionResult> Apply()
        {
            var programs = await _context.AcademicPrograms
                .AsNoTracking()
                .OrderBy(p => p.Level)
                .ThenBy(p => p.Title)
                .ToListAsync();

            var viewModel = new AdmissionApplyViewModel
            {
                Programs = programs
            };

            return View(viewModel);
        }

        // POST: /Admissions/Apply
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(AdmissionApplyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Programs = await _context.AcademicPrograms.AsNoTracking().ToListAsync();
                return View(model);
            }

            var program = await _context.AcademicPrograms.FindAsync(model.AppliedProgramId);
            if (program == null)
            {
                ModelState.AddModelError(nameof(model.AppliedProgramId), "Invalid program selected.");
                model.Programs = await _context.AcademicPrograms.AsNoTracking().ToListAsync();
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

            // Automated KP HED Merit Calculation Engine (Server-side)
            decimal hafizMarks = model.IsHafizQuran ? 20.00m : 0.00m;
            decimal calculatedMerit = 0.00m;

            if (program.Level == "HSSC")
            {
                // HSSC: (SSC Obtained + Hafiz) / SSC Total * 100
                decimal totalSsc = model.SscTotalMarks > 0 ? model.SscTotalMarks : 1100.00m;
                calculatedMerit = Math.Round(((model.SscObtainedMarks + hafizMarks) / totalSsc) * 100m, 2);
            }
            else // BS 4-Year
            {
                // BS: (SSC Obtained / SSC Total * 40) + ((HSSC Obtained + Hafiz) / HSSC Total * 60)
                decimal totalSsc = model.SscTotalMarks > 0 ? model.SscTotalMarks : 1100.00m;
                decimal sscShare = (model.SscObtainedMarks / totalSsc) * 40m;

                decimal hsscObtained = model.HsscObtainedMarks ?? 0m;
                decimal totalHssc = (model.HsscTotalMarks.HasValue && model.HsscTotalMarks.Value > 0) ? model.HsscTotalMarks.Value : 1100.00m;
                decimal hsscShare = ((hsscObtained + hafizMarks) / totalHssc) * 60m;

                calculatedMerit = Math.Round(sscShare + hsscShare, 2);
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
                MeritScore = calculatedMerit,
                AppliedProgramId = model.AppliedProgramId,
                SecondChoiceProgramId = model.SecondChoiceProgramId,
                QuotaCategory = model.QuotaCategory,
                Status = "Pending",
                AppliedDate = DateTime.UtcNow,
                PassportPhotoPath = photoPath,
                DmcScanPath = dmcPath,
                DomicileOrBFormPath = domicilePath
            };

            _context.AdmissionApplications.Add(application);
            await _context.SaveChangesAsync();

            // Invalidate merit list cache
            if (_cache is MemoryCache memCache)
            {
                memCache.Compact(1.0);
            }

            return RedirectToAction(nameof(Confirmation), new { trackingNumber = trackingNo });
        }

        // GET: /Admissions/Confirmation?trackingNumber=GDCM-ADM-2026-0001
        public async Task<IActionResult> Confirmation(string trackingNumber)
        {
            if (string.IsNullOrWhiteSpace(trackingNumber))
            {
                return RedirectToAction(nameof(Apply));
            }

            var application = await _context.AdmissionApplications
                .AsNoTracking()
                .Include(a => a.AppliedProgram)
                    .ThenInclude(p => p!.Department)
                .FirstOrDefaultAsync(a => a.TrackingNumber == trackingNumber.Trim());

            if (application == null)
            {
                return NotFound();
            }

            return View(application);
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

                var apps = await query
                    .OrderByDescending(a => a.MeritScore)
                    .ThenBy(a => a.AppliedDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                decimal cutOff = apps.Any() ? apps.Min(a => a.MeritScore) : 0.00m;

                viewModel = new MeritListFilterViewModel
                {
                    Applications = apps,
                    ProgramId = programId,
                    QuotaCategory = quota,
                    SearchQuery = search,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    CutOffMeritPercentage = cutOff,
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

            var uniqueName = $"{subfolder}_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid().ToString()[..6]}{ext}";
            var fullPath = Path.Combine(uploadsPath, uniqueName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/admissions/{subfolder}/{uniqueName}";
        }
    }
}
