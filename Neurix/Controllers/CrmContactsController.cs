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
    [Route("crm/contacts")]
    public class CrmContactsController : Controller
    {
        private readonly IContactService _contacts;
        private readonly ICompanyService _companies;

        public CrmContactsController(IContactService contacts, ICompanyService companies)
        {
            _contacts = contacts;
            _companies = companies;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(string? search, RecordStatus? status, Guid? companyId, int page = 1)
        {
            var result = await _contacts.GetListAsync(search, status, companyId, page);

            var routeValues = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(search)) routeValues["search"] = search;
            if (status.HasValue) routeValues["status"] = status.Value.ToString();
            if (companyId.HasValue) routeValues["companyId"] = companyId.Value.ToString();

            ViewData["Title"] = "Contacts";

            return View(new CrmContactListViewModel
            {
                Items = result.Items.Select(c => new CrmContactListItemViewModel
                {
                    Id = c.Id,
                    FullName = c.FullName,
                    JobTitle = c.JobTitle,
                    Email = c.Email,
                    Phone = c.Phone,
                    CompanyId = c.CompanyId,
                    CompanyName = c.CompanyName,
                    Status = c.Status
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
                CompanyId = companyId,
                CompanyOptions = await BuildCompanyOptionsAsync()
            });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var contact = await _contacts.GetDetailAsync(id);
            if (contact is null)
            {
                return NotFound();
            }

            ViewData["Title"] = contact.FullName;

            return View(new CrmContactDetailsViewModel
            {
                Id = contact.Id,
                FullName = contact.FullName,
                JobTitle = contact.JobTitle,
                Email = contact.Email,
                Phone = contact.Phone,
                Notes = contact.Notes,
                Status = contact.Status,
                CreatedAtUtc = contact.CreatedAtUtc,
                UpdatedAtUtc = contact.UpdatedAtUtc,
                CompanyId = contact.CompanyId,
                CompanyName = contact.CompanyName,
                CompanyIndustry = contact.CompanyIndustry,
                CompanyEmail = contact.CompanyEmail,
                CompanyPhone = contact.CompanyPhone
            });
        }

        [HttpGet("create")]
        public async Task<IActionResult> Create(Guid? companyId)
        {
            ViewData["Title"] = "New contact";

            return View(new CrmContactFormViewModel
            {
                CompanyId = companyId,
                CompanyOptions = await BuildCompanyOptionsAsync()
            });
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CrmContactFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "New contact";
                model.CompanyOptions = await BuildCompanyOptionsAsync();
                return View(model);
            }

            var id = await _contacts.CreateAsync(ToRequest(model), User.GetUserId());

            TempData["SuccessMessage"] = $"{model.FirstName} {model.LastName} was created.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet("{id:guid}/edit")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var contact = await _contacts.GetForEditAsync(id);
            if (contact is null)
            {
                return NotFound();
            }

            ViewData["Title"] = $"Edit {contact.FirstName} {contact.LastName}";

            return View(new CrmContactFormViewModel
            {
                Id = id,
                FirstName = contact.FirstName,
                LastName = contact.LastName,
                JobTitle = contact.JobTitle,
                Email = contact.Email,
                Phone = contact.Phone,
                CompanyId = contact.CompanyId,
                Notes = contact.Notes,
                Status = contact.Status,
                CompanyOptions = await BuildCompanyOptionsAsync()
            });
        }

        [HttpPost("{id:guid}/edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CrmContactFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Edit contact";
                model.Id = id;
                model.CompanyOptions = await BuildCompanyOptionsAsync();
                return View(model);
            }

            if (!await _contacts.UpdateAsync(id, ToRequest(model)))
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = $"{model.FirstName} {model.LastName} was updated.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost("{id:guid}/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _contacts.SoftDeleteAsync(id);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                return RedirectToAction(nameof(Details), new { id });
            }

            TempData["SuccessMessage"] = "Contact deleted.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<IEnumerable<SelectListItem>> BuildCompanyOptionsAsync()
        {
            var companies = await _companies.GetOptionsAsync();
            return companies
                .Select(c => new SelectListItem(c.Name, c.Id.ToString()))
                .ToList();
        }

        private static ContactRequest ToRequest(CrmContactFormViewModel model) => new()
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            JobTitle = model.JobTitle,
            Email = model.Email,
            Phone = model.Phone,
            Notes = model.Notes,
            CompanyId = model.CompanyId,
            Status = model.Status
        };
    }
}
