using System.ComponentModel.DataAnnotations;

namespace E_commerce.Models
{
    public class CheckOutDTO
    {
        [Required(ErrorMessage ="The Delivery address is required.")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Address must be between 3 and 200 characters.")]
        public string ShippingAddress { get; set; } = "";
        [Required(ErrorMessage = "Please select a payment method.")]
        public string PaymentMethod { get; set; } = "";
    }
}
