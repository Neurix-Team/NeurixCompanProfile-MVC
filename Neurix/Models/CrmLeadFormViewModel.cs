using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Neurix.DAL.Enums;

namespace Neurix.Models
{
    public class CrmLeadFormViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(200)]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Company")]
        public string? CompanyName { get; set; }

        [StringLength(50)]
        [Phone(ErrorMessage = "Enter a valid phone number.")]
        public string? Phone { get; set; }

        public LeadSource Source { get; set; } = LeadSource.Website;

        public LeadStatus Status { get; set; } = LeadStatus.New;

        [StringLength(100)]
        [Display(Name = "Inquiry type")]
        public string? InquiryType { get; set; }

        [StringLength(2000)]
        [Display(Name = "Original message")]
        public string? Message { get; set; }

        [StringLength(2000)]
        public string? Notes { get; set; }

        [Display(Name = "Assigned to")]
        public Guid? AssignedToUserId { get; set; }

        /// <summary>Populated by the controller; never bound from the request.</summary>
        public IEnumerable<SelectListItem> AssignedToOptions { get; set; } = Enumerable.Empty<SelectListItem>();

        /// <summary>
        /// True once the lead has been converted, in which case the status is shown
        /// read-only — it is owned by the conversion action, not this form.
        /// </summary>
        public bool IsConverted { get; set; }
    }
}
