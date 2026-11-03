using College_Web_Application.Models;
using College_Web_Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace College_Web_Application.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Principal,SuperAdmin,Clerk")]
    public class AdmissionSettingsController : Controller
    {
        private readonly IAdmissionSettingsService _settingsService;
        private readonly ILogger<AdmissionSettingsController> _logger;

        public AdmissionSettingsController(
            IAdmissionSettingsService settingsService,
            ILogger<AdmissionSettingsController> logger)
        {
            _settingsService = settingsService;
            _logger = logger;
        }

        // GET: /Admin/AdmissionSettings
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var settings = await _settingsService.GetSettingsAsync();
            return View(settings);
        }

        // POST: /Admin/AdmissionSettings/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(
            bool isBsAdmissionsOpen, 
            bool isFscAdmissionsOpen, 
            string admissionSession, 
            string? closureNotice)
        {
            string updatedBy = User.Identity?.Name ?? "Authorized Admin";
            
            await _settingsService.UpdateSettingsAsync(
                isBsAdmissionsOpen, 
                isFscAdmissionsOpen, 
                admissionSession, 
                closureNotice, 
                updatedBy);

            TempData["SuccessMessage"] = "Admission settings updated successfully. Changes are now reflected live across the public portal.";
            return RedirectToAction(nameof(Index));
        }
    }
}
