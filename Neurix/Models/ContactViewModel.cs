using System.ComponentModel.DataAnnotations;

namespace Neurix.Models
{
    public class ContactViewModel
    {
        [Required(ErrorMessage = "Full Name is required.")]
        [MaxLength(200, ErrorMessage = "Full Name cannot exceed 200 characters.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Work Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        [MaxLength(256, ErrorMessage = "Email cannot exceed 256 characters.")]
        public string Email { get; set; } = string.Empty;

        [MaxLength(200, ErrorMessage = "Company name cannot exceed 200 characters.")]
        public string? CompanyName { get; set; }

        [Required(ErrorMessage = "Inquiry Type is required.")]
        [RegularExpression("^(Demo|Quote|Partnership|Support)$", ErrorMessage = "Invalid inquiry type.")]
        public string InquiryType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Message is required.")]
        [MaxLength(2000, ErrorMessage = "Message cannot exceed 2000 characters.")]
        public string Message { get; set; } = string.Empty;
    }
}
