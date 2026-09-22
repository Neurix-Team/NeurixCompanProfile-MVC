using System.ComponentModel.DataAnnotations;

namespace Neurix.Models
{
    public class NewsletterSubscribeViewModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        [MaxLength(256, ErrorMessage = "Email must be 256 characters or fewer.")]
        public string Email { get; set; } = string.Empty;
    }
}
