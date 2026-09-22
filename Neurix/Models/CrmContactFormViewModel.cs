using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Neurix.DAL.Enums;

namespace Neurix.Models
{
    public class CrmContactFormViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(100)]
        [Display(Name = "First name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(100)]
        [Display(Name = "Last name")]
        public string LastName { get; set; } = string.Empty;

        [StringLength(150)]
        [Display(Name = "Job title")]
        public string? JobTitle { get; set; }

        [StringLength(256)]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string? Email { get; set; }

        [StringLength(50)]
        [Phone(ErrorMessage = "Enter a valid phone number.")]
        public string? Phone { get; set; }

        [Display(Name = "Company")]
        public Guid? CompanyId { get; set; }

        [StringLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters.")]
        public string? Notes { get; set; }

        public RecordStatus Status { get; set; } = RecordStatus.Active;

        /// <summary>Populated by the controller; never bound from the request.</summary>
        public IEnumerable<SelectListItem> CompanyOptions { get; set; } = Enumerable.Empty<SelectListItem>();
    }
}
