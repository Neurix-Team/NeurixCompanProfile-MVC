using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Neurix.BLL.Common;
using Neurix.BLL.Dtos;
using Neurix.BLL.Services;
using Neurix.DAL.Enums;
using Neurix.Extensions;
using Neurix.Models;

namespace Neurix.Controllers
{
    [Authorize(Roles = CrmRoles.AdminOrStaff)]
    [Route("crm/leads")]
    public class CrmLeadsController : Controller
    {
        private readonly ILeadService _leads;
        private readonly ILeadConversionService _conversion;

        public CrmLeadsController(ILeadService leads, ILeadConversionService conversion)
        {
            _leads = leads;
            _conversion = conversion;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(string? search, LeadStatus? status, string? assignedToUserId, int page = 1)
        {
            // "unassigned" is the reception-queue filter (NQ-034): a lead captured from
            // the public form with no owner yet must be findable in one click.
            var unassignedOnly = string.Equals(assignedToUserId, "unassigned", StringComparison.OrdinalIgnoreCase);
            Guid? assignedUser = null;
            if (!unassignedOnly && Guid.TryParse(assignedToUserId, out var parsedUserId))
            {
                assignedUser = parsedUserId;
            }

            var result = await _leads.GetListAsync(search, status, assignedUser, page, unassignedOnly: unassignedOnly);

            var routeValues = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(search)) routeValues["search"] = search;
            if (status.HasValue) routeValues["status"] = status.Value.ToString();
            if (!string.IsNullOrWhiteSpace(assignedToUserId)) routeValues["assignedToUserId"] = assignedToUserId;

            ViewData["Title"] = "Leads";

            return View(new CrmLeadListViewModel
            {
                Items = result.Items.Select(l => new CrmLeadListItemViewModel
                {
                    Id = l.Id,
                    FullName = l.FullName,
                    Email = l.Email,
                    CompanyName = l.CompanyName,
                    Phone = l.Phone,
                    Status = l.Status,
                    Source = l.Source,
                    AssignedToUserName = l.AssignedToUserName,
                    CreatedAtUtc = l.CreatedAtUtc
                }).ToList(),
                Pagination = new CrmPaginationViewModel
                {
                    Page = result.Page,
                    TotalPages = result.TotalPages,
                    TotalCount = result.TotalCount,
                    RouteValues = routeValues
                },
                Search = search,
                Status = status,
                AssignedToUserId = assignedUser,
                UnassignedOnly = unassignedOnly,
                AssignedToOptions = await BuildAssigneeOptionsAsync()
            });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var lead = await _leads.GetDetailAsync(id);
            if (lead is null)
            {
                return NotFound();
            }

            ViewData["Title"] = lead.FullName;

            return View(new CrmLeadDetailsViewModel
            {
                Id = lead.Id,
                FullName = lead.FullName,
                Email = lead.Email,
                CompanyName = lead.CompanyName,
                Phone = lead.Phone,
                Source = lead.Source,
                Status = lead.Status,
                InquiryType = lead.InquiryType,
                Message = lead.Message,
                Notes = lead.Notes,
                AssignedToUserName = lead.AssignedToUserName,
                CreatedAtUtc = lead.CreatedAtUtc,
                UpdatedAtUtc = lead.UpdatedAtUtc,
                CapturedFromWebsiteForm = lead.CapturedFromWebsiteForm,
                IsConverted = lead.IsConverted,
                ConvertedAtUtc = lead.ConvertedAtUtc,
                ConvertedToCompanyId = lead.ConvertedToCompanyId,
                ConvertedToCompanyName = lead.ConvertedToCompanyName,
                ConvertedToContactId = lead.ConvertedToContactId,
                ConvertedToContactName = lead.ConvertedToContactName
            });
        }

        [HttpGet("create")]
        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "New lead";

            return View(new CrmLeadFormViewModel
            {
                AssignedToOptions = await BuildAssigneeOptionsAsync()
            });
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CrmLeadFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "New lead";
                model.AssignedToOptions = await BuildAssigneeOptionsAsync();
                return View(model);
            }

            var id = await _leads.CreateAsync(ToRequest(model), User.GetUserId());

            TempData["SuccessMessage"] = $"{model.FullName} was added as a lead.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet("{id:guid}/edit")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var lead = await _leads.GetForEditAsync(id);
            if (lead is null)
            {
                return NotFound();
            }

            ViewData["Title"] = $"Edit {lead.FullName}";

            return View(new CrmLeadFormViewModel
            {
                Id = id,
                FullName = lead.FullName,
                Email = lead.Email,
                CompanyName = lead.CompanyName,
                Phone = lead.Phone,
                Source = lead.Source,
                Status = lead.Status,
                InquiryType = lead.InquiryType,
                Message = lead.Message,
                Notes = lead.Notes,
                AssignedToUserId = lead.AssignedToUserId,
                IsConverted = lead.Status == LeadStatus.Converted,
                AssignedToOptions = await BuildAssigneeOptionsAsync()
            });
        }

        [HttpPost("{id:guid}/edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CrmLeadFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return await RedisplayEditAsync(id, model);
            }

            switch (await _leads.UpdateAsync(id, ToRequest(model)))
            {
                case LeadUpdateOutcome.NotFound:
                    return NotFound();

                case LeadUpdateOutcome.ConversionNotAllowedHere:
                    ModelState.AddModelError(
                        nameof(model.Status),
                        "Use the Convert action to mark a lead as converted — it creates the company and contact too.");
                    return await RedisplayEditAsync(id, model);

                default:
                    TempData["SuccessMessage"] = $"{model.FullName} was updated.";
                    return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpGet("{id:guid}/convert")]
        public async Task<IActionResult> Convert(Guid id)
        {
            var preview = await _conversion.GetPreviewAsync(id);
            if (preview is null)
            {
                return NotFound();
            }

            ViewData["Title"] = $"Convert {preview.FullName}";

            return View(new CrmLeadConversionViewModel
            {
                LeadId = preview.LeadId,
                FullName = preview.FullName,
                Email = preview.Email,
                Phone = preview.Phone,
                CompanyName = preview.CompanyName,
                SelectedCompanyId = preview.SuggestedCompanyId,
                ExistingContactName = preview.ExistingContactName,
                CompanyOptions = BuildCompanyOptions(preview)
            });
        }

        [HttpPost("{id:guid}/convert")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Convert(Guid id, CrmLeadConversionViewModel model)
        {
            var result = await _conversion.ConvertAsync(id, model.SelectedCompanyId, User.GetUserId());

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                return RedirectToAction(nameof(Details), new { id });
            }

            var converted = result.Value!;

            TempData["SuccessMessage"] = converted.CompanyName is null
                ? $"Lead converted — {converted.ContactName} is now a contact."
                : $"Lead converted — {converted.ContactName} at {converted.CompanyName}.";

            // The contact is the more useful landing spot: it links straight on to
            // the company, but the company page would not say which person this was.
            return RedirectToAction("Details", "CrmContacts", new { id = converted.ContactId });
        }

        [HttpPost("{id:guid}/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _leads.SoftDeleteAsync(id);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                return RedirectToAction(nameof(Details), new { id });
            }

            TempData["SuccessMessage"] = "Lead deleted.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<IActionResult> RedisplayEditAsync(Guid id, CrmLeadFormViewModel model)
        {
            ViewData["Title"] = "Edit lead";

            model.Id = id;
            model.AssignedToOptions = await BuildAssigneeOptionsAsync();

            // Read back the stored status: whether this lead is converted is not
            // something the posted form is allowed to claim.
            var stored = await _leads.GetForEditAsync(id);
            model.IsConverted = stored?.Status == LeadStatus.Converted;

            return View("Edit", model);
        }

        /// <summary>
        /// The company choices for the convert screen. The blank option is the default
        /// action rather than "none": create a company from the lead's own company
        /// name, or — when the lead never named one — leave the contact unattached.
        /// </summary>
        private static IEnumerable<SelectListItem> BuildCompanyOptions(LeadConversionPreview preview)
        {
            var options = new List<SelectListItem>
            {
                new(preview.CompanyName is null
                        ? "— No company —"
                        : $"— Create new company: {preview.CompanyName} —",
                    string.Empty)
            };

            options.AddRange(preview.Companies.Select(c => new SelectListItem(c.Name, c.Id.ToString())));

            return options;
        }

        private async Task<IEnumerable<SelectListItem>> BuildAssigneeOptionsAsync()
        {
            var users = await _leads.GetAssignableUsersAsync();
            return users
                .Select(u => new SelectListItem(u.FullName, u.Id.ToString()))
                .ToList();
        }

        private static LeadRequest ToRequest(CrmLeadFormViewModel model) => new()
        {
            FullName = model.FullName,
            Email = model.Email,
            CompanyName = model.CompanyName,
            Phone = model.Phone,
            Source = model.Source,
            Status = model.Status,
            InquiryType = model.InquiryType,
            Message = model.Message,
            Notes = model.Notes,
            AssignedToUserId = model.AssignedToUserId
        };
    }
}
