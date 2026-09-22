using System.ComponentModel.DataAnnotations;
using Neurix.BLL.Dtos;

namespace Neurix.Models
{
    /// <summary>Profile page: read-only identity facts plus the editable display-name form.</summary>
    public class CrmProfileIndexViewModel
    {
        public CrmUserProfile Profile { get; set; } = new();
        public CrmProfileFormViewModel Form { get; set; } = new();
    }

    /// <summary>Self-service profile form: display name only. Email and roles are not editable here.</summary>
    public class CrmProfileFormViewModel
    {
        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(200, ErrorMessage = "Full name must be 200 characters or fewer.")]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = string.Empty;
    }

    public class CrmChangePasswordFormViewModel
    {
        [Required(ErrorMessage = "Current password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "New password must be at least 8 characters.")]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirmation is required.")]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "The new password and its confirmation do not match.")]
        [Display(Name = "Confirm new password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
