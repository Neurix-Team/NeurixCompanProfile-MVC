using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Neurix.BLL.Dtos;
using Neurix.BLL.Services;
using Neurix.Common;
using Neurix.Models;

namespace Neurix.Controllers
{
    /// <summary>
    /// Footer newsletter form. Subscribes into the CRM database so the address is
    /// actually stored; a failure is reported to the visitor instead of a silent no-op.
    /// </summary>
    public class NewsletterController : Controller
    {
        private readonly INewsletterService _newsletterService;

        public NewsletterController(INewsletterService newsletterService)
        {
            _newsletterService = newsletterService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting(CrmRateLimitPolicies.Newsletter)]
        public async Task<IActionResult> Subscribe(NewsletterSubscribeViewModel model, string? returnUrl)
        {
            if (!ModelState.IsValid)
            {
                TempData["NewsletterResult"] = "invalid";
                return LocalRedirectOrHome(returnUrl);
            }

            var outcome = await _newsletterService.SubscribeAsync(model.Email);

            TempData["NewsletterResult"] = outcome switch
            {
                NewsletterSubscribeOutcome.Subscribed => "subscribed",
                NewsletterSubscribeOutcome.AlreadySubscribed => "already",
                _ => "error"
            };

            return LocalRedirectOrHome(returnUrl);
        }

        // Only ever redirect within this site; the footer renders on every page so
        // the return URL is attacker-controlled in principle.
        private IActionResult LocalRedirectOrHome(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
