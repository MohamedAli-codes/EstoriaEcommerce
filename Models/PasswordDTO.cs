using System.ComponentModel.DataAnnotations;

namespace E_commerce.Models
{
    public class PasswordDTO
    {
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Password must be between 3 and 100 characters.")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New Password is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Password must be between 3 and 100 characters.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm Password is required.")]
        [Compare("NewPassword", ErrorMessage = "Confirm New password and New password are not matching.")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
