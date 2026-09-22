using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Neurix.BLL.Dtos.Cms;

namespace Neurix.Models
{
    public class CmsTestimonialListViewModel
    {
        public Guid? SelectedCompanyId { get; set; }
        public IReadOnlyList<CmsCompanyProfileSummaryDto> Companies { get; set; } = Array.Empty<CmsCompanyProfileSummaryDto>();
        public IReadOnlyList<CmsTestimonialSummaryDto> Testimonials { get; set; } = Array.Empty<CmsTestimonialSummaryDto>();
    }

    public class CmsTestimonialFormViewModel
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "Company brand profile is required.")]
        [Display(Name = "Brand Profile")]
        public Guid CompanyProfileId { get; set; }

        public IEnumerable<SelectListItem> CompanyOptions { get; set; } = Array.Empty<SelectListItem>();

        [Required(ErrorMessage = "English author name is required.")]
        [Display(Name = "Author Name (English)")]
        [StringLength(200)]
        public string NameEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic author name is required.")]
        [Display(Name = "Author Name (Arabic)")]
        [StringLength(200)]
        public string NameAr { get; set; } = string.Empty;

        [Display(Name = "Author Title / Position (English)")]
        [StringLength(200)]
        public string? TitleEn { get; set; }

        [Display(Name = "Author Title / Position (Arabic)")]
        [StringLength(200)]
        public string? TitleAr { get; set; }

        [Display(Name = "Company / University Name")]
        [StringLength(200)]
        public string? CompanyName { get; set; }

        [Display(Name = "Author Photo URL / Path")]
        [StringLength(500)]
        public string? AuthorPhotoPath { get; set; }

        [Display(Name = "Upload Author Photo")]
        public IFormFile? AuthorPhotoFile { get; set; }

        [Required(ErrorMessage = "English quote is required.")]
        [Display(Name = "Quote / Endorsement (English)")]
        [StringLength(1500)]
        public string QuoteEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic quote is required.")]
        [Display(Name = "Quote / Endorsement (Arabic)")]
        [StringLength(1500)]
        public string QuoteAr { get; set; } = string.Empty;

        [Display(Name = "Rating (1 to 5 stars)")]
        [Range(1, 5)]
        public int Rating { get; set; } = 5;

        [Display(Name = "Display Order")]
        [Range(0, 1000)]
        public int DisplayOrder { get; set; } = 1;

        [Display(Name = "Featured on Homepage")]
        public bool IsFeatured { get; set; } = true;

        [Display(Name = "Published (visible on website)")]
        public bool IsPublished { get; set; } = true;
    }

    public class CmsTestimonialDetailsViewModel
    {
        public CmsTestimonialDetailDto Testimonial { get; set; } = new();
    }
}
