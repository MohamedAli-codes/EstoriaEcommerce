using System.ComponentModel.DataAnnotations;

namespace E_commerce.Models
{
    public class ProfileDTO
    {
        [Required(ErrorMessage = "First Name field is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "First name must be between 3 and 100 characters.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last Name field is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Last name must be between 3 and 100 characters.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Email format is not valid.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Email must be between 3 and 100 characters.")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Phone number format is invalid.")]
        [MaxLength(20, ErrorMessage = "Phone number must not exceed 20 characters.")]
        public string? PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Address must be between 3 and 100 characters.")]
        public string Address { get; set; } = string.Empty;


    }
}
