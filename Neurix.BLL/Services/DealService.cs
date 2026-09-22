using Microsoft.EntityFrameworkCore;
using Neurix.BLL.Common;
using Neurix.BLL.Dtos;
using Neurix.DAL.Data;
using Neurix.DAL.Enums;
using Neurix.DAL.Models;

namespace Neurix.BLL.Services
{
    public class DealService : IDealService
    {
        private readonly CrmDbContext _db;

        public DealService(CrmDbContext db)
        {
            _db = db;
        }

        public async Task<PagedResult<DealListRow>> GetListAsync(
            string? search,
            DealStage? stage,
            Guid? companyId,
            int page,
            int pageSize = PagingDefaults.DefaultPageSize)
        {
            page = PagingDefaults.ClampPage(page);
            pageSize = PagingDefaults.ClampPageSize(pageSize);

            var query = _db.Deals.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(d =>
                    d.Name.Contains(term) ||
                    (d.Company != null && d.Company.Name.Contains(term)));
            }

            if (stage.HasValue)
            {
                query = query.Where(d => d.Stage == stage.Value);
            }

            if (companyId.HasValue)
            {
                query = query.Where(d => d.CompanyId == companyId.Value);
            }

            var totalCount = await query.CountAsync();

            // Newest first, with the Id tiebreaker that keeps Skip/Take stable
            // when two deals share a timestamp.
            var items = await query
                .OrderByDescending(d => d.CreatedAtUtc)
                .ThenBy(d => d.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new DealListRow
                {
                    Id = d.Id,
                    Name = d.Name,
                    CompanyId = d.CompanyId,
                    CompanyName = d.Company != null ? d.Company.Name : string.Empty,
                    ContactName = d.Contact != null
                        ? (d.Contact.FirstName + " " + d.Contact.LastName).Trim()
                        : null,
                    Value = d.Value,
                    Stage = d.Stage,
                    AssignedToUserName = d.AssignedToUser != null ? d.AssignedToUser.FullName : null,
                    CreatedAtUtc = d.CreatedAtUtc
                })
                .ToListAsync();

            return new PagedResult<DealListRow>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public Task<DealDetail?> GetDetailAsync(Guid id) =>
            _db.Deals
                .AsNoTracking()
                .Where(d => d.Id == id)
                .Select(d => new DealDetail
                {
                    Id = d.Id,
                    Name = d.Name,
                    Value = d.Value,
                    Stage = d.Stage,
                    Notes = d.Notes,
                    AssignedToUserName = d.AssignedToUser != null ? d.AssignedToUser.FullName : null,
                    CreatedAtUtc = d.CreatedAtUtc,
                    UpdatedAtUtc = d.UpdatedAtUtc,
                    CompanyId = d.CompanyId,
                    CompanyName = d.Company != null ? d.Company.Name : string.Empty,
                    ContactId = d.ContactId,
                    ContactName = d.Contact != null
                        ? (d.Contact.FirstName + " " + d.Contact.LastName).Trim()
                        : null,
                    ContactEmail = d.Contact != null ? d.Contact.Email : null
                })
                .FirstOrDefaultAsync();

        public Task<DealRequest?> GetForEditAsync(Guid id) =>
            _db.Deals
                .AsNoTracking()
                .Where(d => d.Id == id)
                .Select(d => new DealRequest
                {
                    Name = d.Name,
                    CompanyId = d.CompanyId,
                    ContactId = d.ContactId,
                    Value = d.Value,
                    Stage = d.Stage,
                    AssignedToUserId = d.AssignedToUserId,
                    Notes = d.Notes
                })
                .FirstOrDefaultAsync();

        public async Task<Guid> CreateAsync(DealRequest request, Guid currentUserId)
        {
            var deal = new Deal
            {
                Id = Guid.NewGuid(),
                CreatedByUserId = currentUserId,
                IsDeleted = false
            };

            Apply(request, deal);

            _db.Deals.Add(deal);
            await _db.SaveChangesAsync();

            return deal.Id;
        }

        public async Task<bool> UpdateAsync(Guid id, DealRequest request)
        {
            var existing = await _db.Deals.FirstOrDefaultAsync(d => d.Id == id);
            if (existing is null)
            {
                return false;
            }

            Apply(request, existing);

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<ServiceResult> SoftDeleteAsync(Guid id)
        {
            var deal = await _db.Deals.FirstOrDefaultAsync(d => d.Id == id);
            if (deal is null)
            {
                return ServiceResult.Fail("That deal no longer exists.");
            }

            // Nothing references a deal yet, so this is unconditional.
            deal.IsDeleted = true;
            await _db.SaveChangesAsync();

            return ServiceResult.Ok();
        }

        public async Task<IReadOnlyList<ContactOption>> GetContactOptionsAsync() =>
            await _db.Contacts
                .AsNoTracking()
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .Select(c => new ContactOption
                {
                    Id = c.Id,
                    FullName = (c.FirstName + " " + c.LastName).Trim(),
                    CompanyId = c.CompanyId
                })
                .ToListAsync();

        private static void Apply(DealRequest request, Deal deal)
        {
            deal.Name = request.Name.Trim();
            deal.CompanyId = request.CompanyId;
            deal.ContactId = request.ContactId;
            deal.Value = request.Value;
            deal.Stage = request.Stage;
            deal.AssignedToUserId = request.AssignedToUserId;
            deal.Notes = Clean(request.Notes);
        }

        private static string? Clean(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
