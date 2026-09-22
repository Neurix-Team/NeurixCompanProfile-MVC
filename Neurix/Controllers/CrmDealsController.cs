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
    [Route("crm/deals")]
    public class CrmDealsController : Controller
    {
        private readonly IDealService _deals;
        private readonly ICompanyService _companies;
        private readonly ILeadService _leads;

        public CrmDealsController(IDealService deals, ICompanyService companies, ILeadService leads)
        {
            _deals = deals;
            _companies = companies;

            // Reused purely for its assignable-users lookup — the set of staff who
            // can own a record is the same one, so it does not need a second query.
            _leads = leads;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(string? search, DealStage? stage, Guid? companyId, int page = 1)
        {
            var result = await _deals.GetListAsync(search, stage, companyId, page);

            var routeValues = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(search)) routeValues["search"] = search;
            if (stage.HasValue) routeValues["stage"] = stage.Value.ToString();
            if (companyId.HasValue) routeValues["companyId"] = companyId.Value.ToString();

            ViewData["Title"] = "Deals";

            return View(new CrmDealListViewModel
            {
                Items = result.Items.Select(d => new CrmDealListItemViewModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    CompanyId = d.CompanyId,
                    CompanyName = d.CompanyName,
                    ContactName = d.ContactName,
                    Value = d.Value,
                    Stage = d.Stage,
                    AssignedToUserName = d.AssignedToUserName,
                    CreatedAtUtc = d.CreatedAtUtc
                }).ToList(),
                Pagination = new CrmPaginationViewModel
                {
                    Page = result.Page,
                    TotalPages = result.TotalPages,
                    TotalCount = result.TotalCount,
                    RouteValues = routeValues
                },
                Search = search,
                Stage = stage,
                CompanyId = companyId,
                CompanyOptions = await BuildCompanyOptionsAsync()
            });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var deal = await _deals.GetDetailAsync(id);
            if (deal is null)
            {
                return NotFound();
            }

            ViewData["Title"] = deal.Name;

            return View(new CrmDealDetailsViewModel
            {
                Id = deal.Id,
                Name = deal.Name,
                Value = deal.Value,
                Stage = deal.Stage,
                Notes = deal.Notes,
                AssignedToUserName = deal.AssignedToUserName,
                CreatedAtUtc = deal.CreatedAtUtc,
                UpdatedAtUtc = deal.UpdatedAtUtc,
                CompanyId = deal.CompanyId,
                CompanyName = deal.CompanyName,
                ContactId = deal.ContactId,
                ContactName = deal.ContactName,
                ContactEmail = deal.ContactEmail
            });
        }

        /// <summary>
        /// <paramref name="companyId"/> and <paramref name="contactId"/> pre-fill the
        /// form when arriving from a contact's "New deal" quick action.
        /// </summary>
        [HttpGet("create")]
        public async Task<IActionResult> Create(Guid? companyId, Guid? contactId)
        {
            ViewData["Title"] = "New deal";

            var model = new CrmDealFormViewModel
            {
                CompanyId = companyId,
                ContactId = contactId
            };

            await PopulateOptionsAsync(model);
            return View(model);
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CrmDealFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "New deal";
                await PopulateOptionsAsync(model);
                return View(model);
            }

            var id = await _deals.CreateAsync(ToRequest(model), User.GetUserId());

            TempData["SuccessMessage"] = $"{model.Name} was added.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet("{id:guid}/edit")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var deal = await _deals.GetForEditAsync(id);
            if (deal is null)
            {
                return NotFound();
            }

            ViewData["Title"] = $"Edit {deal.Name}";

            var model = new CrmDealFormViewModel
            {
                Id = id,
                Name = deal.Name,
                CompanyId = deal.CompanyId,
                ContactId = deal.ContactId,
                Value = deal.Value,
                Stage = deal.Stage,
                AssignedToUserId = deal.AssignedToUserId,
                Notes = deal.Notes
            };

            await PopulateOptionsAsync(model);
            return View(model);
        }

        [HttpPost("{id:guid}/edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CrmDealFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Edit deal";
                model.Id = id;
                await PopulateOptionsAsync(model);
                return View(model);
            }

            if (!await _deals.UpdateAsync(id, ToRequest(model)))
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = $"{model.Name} was updated.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost("{id:guid}/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _deals.SoftDeleteAsync(id);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                return RedirectToAction(nameof(Details), new { id });
            }

            TempData["SuccessMessage"] = "Deal deleted.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateOptionsAsync(CrmDealFormViewModel model)
        {
            model.CompanyOptions = await BuildCompanyOptionsAsync();

            // Every contact stays selectable, with its company shown in the label so
            // a mismatch is visible at a glance. Filtering the list to the chosen
            // company would need client-side state for no real MVP gain.
            var companyNames = (await _companies.GetOptionsAsync())
                .ToDictionary(c => c.Id, c => c.Name);

            model.ContactOptions = (await _deals.GetContactOptionsAsync())
                .Select(c => new SelectListItem(
                    c.CompanyId.HasValue && companyNames.TryGetValue(c.CompanyId.Value, out var name)
                        ? $"{c.FullName} — {name}"
                        : c.FullName,
                    c.Id.ToString()))
                .ToList();

            var users = await _leads.GetAssignableUsersAsync();
            model.AssignedToOptions = users
                .Select(u => new SelectListItem(u.FullName, u.Id.ToString()))
                .ToList();
        }

        private async Task<IEnumerable<SelectListItem>> BuildCompanyOptionsAsync()
        {
            var companies = await _companies.GetOptionsAsync();
            return companies
                .Select(c => new SelectListItem(c.Name, c.Id.ToString()))
                .ToList();
        }

        private static DealRequest ToRequest(CrmDealFormViewModel model) => new()
        {
            Name = model.Name,
            // Guarded by [Required] on the view model, so ModelState has already
            // rejected the request by the time this runs with a null company.
            CompanyId = model.CompanyId!.Value,
            ContactId = model.ContactId,
            Value = model.Value,
            Stage = model.Stage,
            AssignedToUserId = model.AssignedToUserId,
            Notes = model.Notes
        };
    }
}
