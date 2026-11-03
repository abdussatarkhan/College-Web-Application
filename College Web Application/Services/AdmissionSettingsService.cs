using College_Web_Application.Data;
using College_Web_Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace College_Web_Application.Services
{
    public class AdmissionSettingsService : IAdmissionSettingsService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AdmissionSettingsService> _logger;
        private const string CacheKey = "GDC_AdmissionSettings_State";

        public AdmissionSettingsService(
            ApplicationDbContext context,
            IMemoryCache cache,
            ILogger<AdmissionSettingsService> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        public async Task<AdmissionSettings> GetSettingsAsync()
        {
            if (_cache.TryGetValue(CacheKey, out AdmissionSettings? cached) && cached != null)
            {
                return cached;
            }

            var settings = await _context.AdmissionSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new AdmissionSettings
                {
                    Id = 1,
                    IsBsAdmissionsOpen = true,
                    IsFscAdmissionsOpen = true,
                    AdmissionSession = "Academic Session 2026-2027",
                    ClosureNotice = "Online admissions are currently closed by order of the Principal's Office. Stay tuned for future announcements.",
                    LastUpdated = DateTime.UtcNow,
                    UpdatedBy = "System Default"
                };

                _context.AdmissionSettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            _cache.Set(CacheKey, settings, TimeSpan.FromMinutes(5));
            return settings;
        }

        public async Task<AdmissionSettings> UpdateSettingsAsync(
            bool isBsOpen, 
            bool isFscOpen, 
            string session, 
            string? closureNotice, 
            string updatedBy)
        {
            var settings = await _context.AdmissionSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new AdmissionSettings();
                _context.AdmissionSettings.Add(settings);
            }

            settings.IsBsAdmissionsOpen = isBsOpen;
            settings.IsFscAdmissionsOpen = isFscOpen;
            settings.AdmissionSession = string.IsNullOrWhiteSpace(session) ? "Academic Session 2026-2027" : session.Trim();
            settings.ClosureNotice = closureNotice?.Trim();
            settings.LastUpdated = DateTime.UtcNow;
            settings.UpdatedBy = updatedBy;

            await _context.SaveChangesAsync();
            _cache.Remove(CacheKey);

            _logger.LogInformation("Admissions configuration updated by {User}. BS Open: {BsOpen}, FSc Open: {FscOpen}, Session: {Session}",
                updatedBy, isBsOpen, isFscOpen, session);

            return settings;
        }
    }
}
