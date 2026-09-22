using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neurix.BLL.Common;
using Neurix.BLL.Dtos;
using Neurix.BLL.Services;
using Neurix.DAL.Enums;
using Neurix.Extensions;
using Neurix.Models;

namespace Neurix.Controllers
{
    [Authorize(Roles = CrmRoles.AdminOrStaff)]
    [Route("crm/companies")]
    public class CrmCompaniesController : Controller
    {
        private readonly ICompanyService _companies;

        public CrmCompaniesController(ICompanyService companies)
        {
            _companies = companies;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(string? search, RecordStatus? status, int page = 1)
        {
            var result = await _companies.GetListAsync(search, status, page);

            var routeValues = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(search)) routeValues["search"] = search;
            if (status.HasValue) routeValues["status"] = status.Value.ToString();

            ViewData["Title"] = "Companies";

            return View(new CrmCompanyListViewModel
            {
                Items = result.Items.Select(c => new CrmCompanyListItemViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Industry = c.Industry,
                    City = c.City,
                    Email = c.Email,
                    Phone = c.Phone,
                    ContactCount = c.ContactCount,
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
                Status = status
            });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var company = await _companies.GetDetailAsync(id);
            if (company is null)
            {
                return NotFound();
            }

            ViewData["Title"] = company.Name;

            return View(new CrmCompanyDetailsViewModel
            {
                Id = company.Id,
                Name = company.Name,
                Industry = company.Industry,
                Website = company.Website,
                Phone = company.Phone,
                Email = company.Email,
                Address = company.Address,
                City = company.City,
                Country = company.Country,
                Description = company.Description,
                Status = company.Status,
                CreatedAtUtc = company.CreatedAtUtc,
                UpdatedAtUtc = company.UpdatedAtUtc,
                Contacts = company.Contacts.Select(x => new CrmCompanyContactViewModel
                {
                    Id = x.Id,
                    FullName = x.FullName,
                    JobTitle = x.JobTitle,
                    Email = x.Email
                }).ToList()
            });
        }

        [HttpGet("create")]
        public IActionResult Create()
        {
            ViewData["Title"] = "New company";
            return View(new CrmCompanyFormViewModel());
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CrmCompanyFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "New company";
                return View(model);
            }

            var id = await _companies.CreateAsync(ToRequest(model), User.GetUserId());

            TempData["SuccessMessage"] = $"{model.Name} was created.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet("{id:guid}/edit")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var company = await _companies.GetForEditAsync(id);
            if (company is null)
            {
                return NotFound();
            }

            ViewData["Title"] = $"Edit {company.Name}";

            return View(new CrmCompanyFormViewModel
            {
                Id = id,
                Name = company.Name,
                Industry = company.Industry,
                Website = company.Website,
                Phone = company.Phone,
                Email = company.Email,
                Address = company.Address,
                City = company.City,
                Country = company.Country,
                Description = company.Description,
                Status = company.Status
            });
        }

        [HttpPost("{id:guid}/edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CrmCompanyFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Edit company";
                model.Id = id;
                return View(model);
            }

            if (!await _companies.UpdateAsync(id, ToRequest(model)))
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
            var result = await _companies.SoftDeleteAsync(id);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                return RedirectToAction(nameof(Details), new { id });
            }

            TempData["SuccessMessage"] = "Company deleted.";
            return RedirectToAction(nameof(Index));
        }

        private static CompanyRequest ToRequest(CrmCompanyFormViewModel model) => new()
        {
            Name = model.Name,
            Industry = model.Industry,
            Website = model.Website,
            Phone = model.Phone,
            Email = model.Email,
            Address = model.Address,
            City = model.City,
            Country = model.Country,
            Description = model.Description,
            Status = model.Status
        };
    }
}
