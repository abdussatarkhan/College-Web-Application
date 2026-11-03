using College_Web_Application.Models;

namespace College_Web_Application.Services
{
    public interface IAdmissionSettingsService
    {
        Task<AdmissionSettings> GetSettingsAsync();
        Task<AdmissionSettings> UpdateSettingsAsync(bool isBsOpen, bool isFscOpen, string session, string? closureNotice, string updatedBy);
    }
}
