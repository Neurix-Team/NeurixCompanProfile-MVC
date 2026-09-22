using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Neurix.DAL.Enums;

namespace Neurix.Models
{
    public class CrmDealFormViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Deal name is required.")]
        [StringLength(200)]
        [Display(Name = "Deal name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Pick the company this deal belongs to.")]
        [Display(Name = "Company")]
        public Guid? CompanyId { get; set; }

        [Display(Name = "Primary contact")]
        public Guid? ContactId { get; set; }

        [Range(0, 999999999, ErrorMessage = "Value must be zero or more.")]
        [Display(Name = "Value")]
        public decimal Value { get; set; }

        public DealStage Stage { get; set; } = DealStage.New;

        [Display(Name = "Assigned to")]
        public Guid? AssignedToUserId { get; set; }

        [StringLength(2000)]
        public string? Notes { get; set; }

        /// <summary>Populated by the controller; never bound from the request.</summary>
        public IEnumerable<SelectListItem> CompanyOptions { get; set; } = Enumerable.Empty<SelectListItem>();

        /// <summary>Populated by the controller; never bound from the request.</summary>
        public IEnumerable<SelectListItem> ContactOptions { get; set; } = Enumerable.Empty<SelectListItem>();

        /// <summary>Populated by the controller; never bound from the request.</summary>
        public IEnumerable<SelectListItem> AssignedToOptions { get; set; } = Enumerable.Empty<SelectListItem>();
    }
}
