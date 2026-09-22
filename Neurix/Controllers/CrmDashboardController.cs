using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neurix.BLL.Common;
using Neurix.BLL.Services;
using Neurix.Extensions;

namespace Neurix.Controllers
{
    [Authorize(Roles = CrmRoles.AdminOrStaff)]
    [Route("crm/dashboard")]
    public class CrmDashboardController : Controller
    {
        private readonly ICrmUserService _users;

        public CrmDashboardController(ICrmUserService users)
        {
            _users = users;
        }

        [HttpGet("")]
        [HttpGet("/crm")]
        public async Task<IActionResult> Index()
        {
            var profile = await _users.GetProfileAsync(User.GetUserId());

            ViewData["Title"] = "Dashboard";
            ViewData["FullName"] = profile?.FullName ?? User.Identity?.Name ?? "CRM User";
            ViewData["Email"] = profile?.Email ?? string.Empty;
            ViewData["Roles"] = profile?.Roles.ToArray() ?? Array.Empty<string>();

            return View();
        }
    }
}
