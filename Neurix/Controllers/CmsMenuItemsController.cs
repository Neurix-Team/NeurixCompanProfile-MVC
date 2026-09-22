using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Neurix.BLL.Common;
using Neurix.BLL.Dtos.Cms;
using Neurix.BLL.Services.Cms;
using Neurix.DAL.Models;
using Neurix.Models;

namespace Neurix.Controllers
{
    [Authorize(Roles = CrmRoles.Admin)]
    [Route("cms/menu-items")]
    public class CmsMenuItemsController : Controller
    {
        private readonly ICmsMenuItemService _menuItemService;
        private readonly ICmsCompanyProfileService _profileService;
        private readonly ILogger<CmsMenuItemsController> _logger;

        public CmsMenuItemsController(
            ICmsMenuItemService menuItemService,
            ICmsCompanyProfileService profileService,
            ILogger<CmsMenuItemsController> logger)
        {
            _menuItemService = menuItemService;
            _profileService = profileService;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index([FromQuery] Guid? companyId, [FromQuery] CmsMenuItemPlacement? placement)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            var selectedCompany = companyId.HasValue
                ? companies.FirstOrDefault(c => c.Id == companyId.Value)
                : companies.FirstOrDefault();

            var selectedId = selectedCompany?.Id ?? Guid.Empty;
            var items = selectedId != Guid.Empty
                ? await _menuItemService.GetMenuItemsByCompanyIdAsync(selectedId, placement, includeUnpublished: true)
                : Array.Empty<CmsMenuItemSummaryDto>();

            var model = new CmsMenuItemListViewModel
            {
                SelectedCompanyId = selectedId,
                SelectedPlacement = placement,
                Companies = companies,
                CompanyOptions = companies.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.NameEn} ({c.Slug})",
                    Selected = c.Id == selectedId
                }),
                MenuItems = items
            };

            return View(model);
        }

        [HttpGet("create")]
        public async Task<IActionResult> Create([FromQuery] Guid? companyId)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            var selectedId = companyId ?? companies.FirstOrDefault()?.Id ?? Guid.Empty;

            var model = new CmsMenuItemFormViewModel
            {
                CompanyProfileId = selectedId,
                CompanyOptions = companies.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.NameEn} ({c.Slug})",
                    Selected = c.Id == selectedId
                }),
                Placement = CmsMenuItemPlacement.Header,
                DisplayOrder = 1,
                IsPublished = true
            };

            return View(model);
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CmsMenuItemFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCompanyOptionsAsync(model);
                return View(model);
            }

            var dto = new CmsMenuItemUpsertDto
            {
                CompanyProfileId = model.CompanyProfileId,
                LabelEn = model.LabelEn,
                LabelAr = model.LabelAr,
                Url = model.Url,
                IconName = model.IconName,
                Placement = model.Placement,
                DisplayOrder = model.DisplayOrder,
                OpenInNewTab = model.OpenInNewTab,
                IsPublished = model.IsPublished
            };

            var userId = GetCurrentUserId();
            var result = await _menuItemService.CreateMenuItemAsync(dto, userId);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to create menu item.");
                await PopulateCompanyOptionsAsync(model);
                return View(model);
            }

            TempData["SuccessMessage"] = "Menu item created successfully.";
            return RedirectToAction(nameof(Index), new { companyId = model.CompanyProfileId });
        }

        [HttpGet("edit/{id:guid}")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var item = await _menuItemService.GetMenuItemByIdAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);

            var model = new CmsMenuItemFormViewModel
            {
                Id = item.Id,
                CompanyProfileId = item.CompanyProfileId,
                CompanyOptions = companies.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.NameEn} ({c.Slug})",
                    Selected = c.Id == item.CompanyProfileId
                }),
                LabelEn = item.LabelEn,
                LabelAr = item.LabelAr,
                Url = item.Url,
                IconName = item.IconName,
                Placement = item.Placement,
                DisplayOrder = item.DisplayOrder,
                OpenInNewTab = item.OpenInNewTab,
                IsPublished = item.IsPublished
            };

            return View(model);
        }

        [HttpPost("edit/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CmsMenuItemFormViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                await PopulateCompanyOptionsAsync(model);
                return View(model);
            }

            var dto = new CmsMenuItemUpsertDto
            {
                Id = model.Id,
                CompanyProfileId = model.CompanyProfileId,
                LabelEn = model.LabelEn,
                LabelAr = model.LabelAr,
                Url = model.Url,
                IconName = model.IconName,
                Placement = model.Placement,
                DisplayOrder = model.DisplayOrder,
                OpenInNewTab = model.OpenInNewTab,
                IsPublished = model.IsPublished
            };

            var userId = GetCurrentUserId();
            var result = await _menuItemService.UpdateMenuItemAsync(dto, userId);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to update menu item.");
                await PopulateCompanyOptionsAsync(model);
                return View(model);
            }

            TempData["SuccessMessage"] = "Menu item updated successfully.";
            return RedirectToAction(nameof(Index), new { companyId = model.CompanyProfileId });
        }

        [HttpPost("delete/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id, [FromQuery] Guid? companyId)
        {
            var userId = GetCurrentUserId();
            var result = await _menuItemService.DeleteMenuItemAsync(id, userId);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Failed to delete menu item.";
            }
            else
            {
                TempData["SuccessMessage"] = "Menu item deleted successfully.";
            }

            return RedirectToAction(nameof(Index), new { companyId });
        }

        [HttpPost("reorder")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reorder([FromBody] List<Guid> orderedIds)
        {
            var userId = GetCurrentUserId();
            var result = await _menuItemService.ReorderMenuItemsAsync(orderedIds, userId);

            if (!result.Success)
            {
                return Json(new { success = false, message = result.ErrorMessage });
            }

            return Json(new { success = true });
        }

        private async Task PopulateCompanyOptionsAsync(CmsMenuItemFormViewModel model)
        {
            var companies = await _profileService.GetAllProfilesAsync(includeUnpublished: true);
            model.CompanyOptions = companies.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.NameEn} ({c.Slug})",
                Selected = c.Id == model.CompanyProfileId
            });
        }

        private Guid? GetCurrentUserId()
        {
            var claim = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(claim, out var id) ? id : null;
        }
    }
}
