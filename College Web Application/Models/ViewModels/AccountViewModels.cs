using System.ComponentModel.DataAnnotations;

namespace College_Web_Application.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Roll Number or Institutional Email")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember this device")]
        public bool RememberMe { get; set; } = true;

        public string? ReturnUrl { get; set; }
    }
}
