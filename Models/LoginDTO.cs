using System.ComponentModel.DataAnnotations;

namespace E_commerce.Models
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Email format is not valid.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Email must be between 3 and 100 characters.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Password must be between 3 and 100 characters.")]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; } //kont nseha deh 
    }
}
