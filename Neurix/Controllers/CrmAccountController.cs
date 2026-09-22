using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Neurix.BLL.Dtos;
using Neurix.BLL.Services;
using Neurix.Common;
using Neurix.Models;

namespace Neurix.Controllers
{
    [AllowAnonymous]
    [Route("crm/account")]
    public class CrmAccountController : Controller
    {
        private readonly ICrmAuthService _auth;

        public CrmAccountController(ICrmAuthService auth)
        {
            _auth = auth;
        }

        [HttpGet("login")]
        public IActionResult Login(string? returnUrl = null, bool throttled = false)
        {
            if (_auth.IsSignedIn(User))
            {
                return RedirectToAction("Index", "CrmDashboard");
            }

            if (throttled)
            {
                ModelState.AddModelError(string.Empty,
                    "Too many sign-in attempts. Please wait a minute and try again.");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new CrmLoginViewModel());
        }

        [HttpPost("login")]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting(CrmRateLimitPolicies.Login)]
        public async Task<IActionResult> Login(CrmLoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var outcome = await _auth.SignInAsync(model.Email, model.Password, model.RememberMe);

            switch (outcome)
            {
                case CrmSignInOutcome.Success:
                    // Only ever redirect within this site.
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    return RedirectToAction("Index", "CrmDashboard");

                case CrmSignInOutcome.Unavailable:
                    ModelState.AddModelError(string.Empty,
                        "The CRM service is temporarily unavailable. Please try again later.");
                    return View(model);

                // LockedOut deliberately shows the exact same message as an invalid
                // email/password: a distinct "this account is locked" reply would let an
                // anonymous caller tell which email addresses have a real account.
                case CrmSignInOutcome.LockedOut:
                default:
                    ModelState.AddModelError(string.Empty, "Invalid email or password.");
                    return View(model);
            }
        }

        [HttpPost("logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _auth.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        [HttpGet("access-denied")]
        public IActionResult AccessDenied() => View();
    }
}
