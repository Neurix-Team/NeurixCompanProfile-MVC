using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neurix.BLL.Dtos;
using Neurix.BLL.Services;
using Neurix.Models;

namespace Neurix.Controllers
{
    /// <summary>
    /// Self-service account pages for signed-in CRM users: view/edit the display
    /// name and change the password. The identity claims provide the user id, so
    /// nothing here trusts an id from the request.
    /// </summary>
    [Authorize]
    [Route("crm/profile")]
    public class CrmProfileController : Controller
    {
        private readonly ICrmUserService _users;
        private readonly ICrmAuthService _auth;

        public CrmProfileController(ICrmUserService users, ICrmAuthService auth)
        {
            _users = users;
            _auth = auth;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var profile = await _users.GetProfileAsync(CurrentUserId);

            if (profile is null)
            {
                return RedirectToAction("AccessDenied", "CrmAccount");
            }

            var model = new CrmProfileIndexViewModel
            {
                Profile = profile,
                Form = new CrmProfileFormViewModel { FullName = profile.FullName }
            };

            return View(model);
        }

        [HttpPost("update")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(CrmProfileFormViewModel form)
        {
            if (!ModelState.IsValid)
            {
                return await ReloadIndexWithError(form);
            }

            var outcome = await _users.UpdateFullNameAsync(CurrentUserId, form.FullName);

            switch (outcome)
            {
                case CrmProfileUpdateOutcome.Updated:
                    TempData["SuccessMessage"] = "Your profile has been saved.";
                    return RedirectToAction(nameof(Index));

                case CrmProfileUpdateOutcome.NotFound:
                    return RedirectToAction("AccessDenied", "CrmAccount");

                case CrmProfileUpdateOutcome.InvalidFullName:
                    ModelState.AddModelError(nameof(form.FullName),
                        "Enter a full name of 1–200 characters.");
                    break;

                default:
                    ModelState.AddModelError(string.Empty,
                        "Your profile could not be saved. Please try again.");
                    break;
            }

            return await ReloadIndexWithError(form);
        }

        [HttpGet("change-password")]
        public IActionResult ChangePassword()
        {
            return View(new CrmChangePasswordFormViewModel());
        }

        [HttpPost("change-password")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(CrmChangePasswordFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var outcome = await _users.ChangePasswordAsync(
                CurrentUserId, model.CurrentPassword, model.NewPassword);

            switch (outcome)
            {
                case CrmPasswordChangeOutcome.Success:
                    // The service already rotated the security stamp so every OTHER
                    // session is invalidated; refresh this one so the user who just
                    // changed their own password is not logged out by their own change.
                    await _auth.RefreshSignInAsync(CurrentUserId);
                    TempData["SuccessMessage"] = "Your password has been changed.";
                    return RedirectToAction(nameof(Index));

                case CrmPasswordChangeOutcome.NotFound:
                    return RedirectToAction("AccessDenied", "CrmAccount");

                case CrmPasswordChangeOutcome.IncorrectCurrentPassword:
                    ModelState.AddModelError(nameof(model.CurrentPassword),
                        "Your current password is not correct.");
                    break;

                case CrmPasswordChangeOutcome.InvalidNewPassword:
                    ModelState.AddModelError(nameof(model.NewPassword),
                        "The new password does not meet the password policy (at least 8 characters).");
                    break;

                default:
                    ModelState.AddModelError(string.Empty,
                        "Your password could not be changed. Please try again.");
                    break;
            }

            return View(model);
        }

        private async Task<IActionResult> ReloadIndexWithError(CrmProfileFormViewModel form)
        {
            var profile = await _users.GetProfileAsync(CurrentUserId);

            if (profile is null)
            {
                return RedirectToAction("AccessDenied", "CrmAccount");
            }

            return View("Index", new CrmProfileIndexViewModel
            {
                Profile = profile,
                Form = form
            });
        }

        private Guid CurrentUserId =>
            Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? throw new InvalidOperationException("A signed-in CRM user has no name identifier claim."));
    }
}
