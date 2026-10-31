using College_Web_Application.Data;
using College_Web_Application.Models;
using College_Web_Application.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace College_Web_Application.Controllers
{
    public class NoticesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IMemoryCache _cache;
        private readonly ILogger<NoticesController> _logger;

        private const string CacheKeyPrefix = "GDC_Notices_";

        public NoticesController(
            ApplicationDbContext context,
            IWebHostEnvironment environment,
            IMemoryCache cache,
            ILogger<NoticesController> logger)
        {
            _context = context;
            _environment = environment;
            _cache = cache;
            _logger = logger;
        }

        // GET: /Notices
        public async Task<IActionResult> Index(string category = "All", string? search = null)
        {
            string cacheKey = $"{CacheKeyPrefix}{category}_{search?.Trim().ToLower() ?? "none"}";

            if (!_cache.TryGetValue(cacheKey, out NoticeListViewModel? viewModel))
            {
                var query = _context.Notices.AsNoTracking().AsQueryable();

                if (!string.IsNullOrWhiteSpace(category) && category != "All")
                {
                    query = query.Where(n => n.Category.ToLower() == category.Trim().ToLower());
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var keyword = search.Trim().ToLower();
                    query = query.Where(n => n.Title.ToLower().Contains(keyword) || n.Description.ToLower().Contains(keyword));
                }

                var notices = await query
                    .OrderByDescending(n => n.IsPinned)
                    .ThenByDescending(n => n.PublishedDate)
                    .ToListAsync();

                viewModel = new NoticeListViewModel
                {
                    Notices = notices,
                    SelectedCategory = category,
                    SearchKeyword = search,
                    TotalCount = notices.Count
                };

                // Cache for 10 minutes to protect against 4G traffic spikes
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10))
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));

                _cache.Set(cacheKey, viewModel, cacheEntryOptions);
            }

            return View(viewModel);
        }

        // GET: /Notices/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var notice = await _context.Notices.AsNoTracking().FirstOrDefaultAsync(n => n.Id == id);
            if (notice == null)
            {
                return NotFound();
            }

            return View(notice);
        }

        // GET: /Notices/Create (Admin, Principal, Clerk)
        [Authorize(Roles = "SuperAdmin,Principal,Clerk")]
        public IActionResult Create()
        {
            return View(new CreateNoticeViewModel());
        }

        // POST: /Notices/Create (Admin, Principal, Clerk)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Principal,Clerk")]
        public async Task<IActionResult> Create(CreateNoticeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string? attachmentUrl = null;

            if (model.PdfFile != null && model.PdfFile.Length > 0)
            {
                var ext = Path.GetExtension(model.PdfFile.FileName).ToLower();
                if (ext != ".pdf")
                {
                    ModelState.AddModelError(nameof(model.PdfFile), "Only official PDF documents (.pdf) are allowed as attachments.");
                    return View(model);
                }

                if (model.PdfFile.Length > 5 * 1024 * 1024) // 5MB limit
                {
                    ModelState.AddModelError(nameof(model.PdfFile), "Attachment size cannot exceed 5 Megabytes (MB).");
                    return View(model);
                }

                var uploadsDir = Path.Combine(_environment.WebRootPath, "uploads", "notices");
                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }

                var uniqueFileName = $"notice_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString()[..8]}{ext}";
                var filePath = Path.Combine(uploadsDir, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.PdfFile.CopyToAsync(stream);
                }

                attachmentUrl = $"/uploads/notices/{uniqueFileName}";
            }

            var notice = new Notice
            {
                Title = model.Title.Trim(),
                Category = model.Category,
                Description = model.Description.Trim(),
                AttachmentPdfUrl = attachmentUrl,
                PublishedDate = DateTime.UtcNow,
                IsPinned = model.IsPinned
            };

            _context.Notices.Add(notice);
            await _context.SaveChangesAsync();

            // Clear cache
            InvalidateNoticesCache();

            TempData["SuccessMessage"] = $"Institutional notice '{notice.Title}' published successfully to the digital notice board!";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Notices/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Principal,Clerk")]
        public async Task<IActionResult> Delete(int id)
        {
            var notice = await _context.Notices.FindAsync(id);
            if (notice != null)
            {
                _context.Notices.Remove(notice);
                await _context.SaveChangesAsync();
                InvalidateNoticesCache();
                TempData["SuccessMessage"] = "Notice removed successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        private void InvalidateNoticesCache()
        {
            if (_cache is MemoryCache memCache)
            {
                memCache.Compact(1.0); // Clear cached entries
            }
        }
    }
}
