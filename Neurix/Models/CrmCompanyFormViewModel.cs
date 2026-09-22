using System.ComponentModel.DataAnnotations;
using Neurix.DAL.Enums;

namespace Neurix.Models
{
    public class CrmCompanyFormViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Company name is required.")]
        [StringLength(200, ErrorMessage = "Company name cannot exceed 200 characters.")]
        [Display(Name = "Company name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Industry { get; set; }

        [StringLength(300)]
        [Url(ErrorMessage = "Enter a valid URL, including https://")]
        public string? Website { get; set; }

        [StringLength(50)]
        [Phone(ErrorMessage = "Enter a valid phone number.")]
        public string? Phone { get; set; }

        [StringLength(256)]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string? Email { get; set; }

        [StringLength(300)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }

        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
        public string? Description { get; set; }

        public RecordStatus Status { get; set; } = RecordStatus.Active;
    }
}
