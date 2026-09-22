using Microsoft.EntityFrameworkCore;
using Neurix.BLL.Common;
using Neurix.BLL.Dtos;
using Neurix.DAL.Data;
using Neurix.DAL.Enums;
using Neurix.DAL.Models;

namespace Neurix.BLL.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly CrmDbContext _db;

        public CompanyService(CrmDbContext db)
        {
            _db = db;
        }

        public async Task<PagedResult<CompanyListRow>> GetListAsync(
            string? search,
            RecordStatus? status,
            int page,
            int pageSize = PagingDefaults.DefaultPageSize)
        {
            page = PagingDefaults.ClampPage(page);
            pageSize = PagingDefaults.ClampPageSize(pageSize);

            var query = _db.Companies.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(c =>
                    c.Name.Contains(term) ||
                    (c.Email != null && c.Email.Contains(term)) ||
                    (c.Phone != null && c.Phone.Contains(term)) ||
                    (c.City != null && c.City.Contains(term)));
            }

            if (status.HasValue)
            {
                query = query.Where(c => c.Status == status.Value);
            }

            var totalCount = await query.CountAsync();

            // Ordering must be deterministic or Skip/Take can repeat or drop rows.
            var items = await query
                .OrderBy(c => c.Name)
                .ThenBy(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CompanyListRow
                {
                    Id = c.Id,
                    Name = c.Name,
                    Industry = c.Industry,
                    City = c.City,
                    Email = c.Email,
                    Phone = c.Phone,
                    Status = c.Status,
                    ContactCount = c.Contacts.Count(x => !x.IsDeleted)
                })
                .ToListAsync();

            return new PagedResult<CompanyListRow>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public Task<CompanyDetail?> GetDetailAsync(Guid id) =>
            _db.Companies
                .AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new CompanyDetail
                {
                    Id = c.Id,
                    Name = c.Name,
                    Industry = c.Industry,
                    Website = c.Website,
                    Phone = c.Phone,
                    Email = c.Email,
                    Address = c.Address,
                    City = c.City,
                    Country = c.Country,
                    Description = c.Description,
                    Status = c.Status,
                    CreatedAtUtc = c.CreatedAtUtc,
                    UpdatedAtUtc = c.UpdatedAtUtc,
                    Contacts = c.Contacts
                        .Where(x => !x.IsDeleted)
                        .OrderBy(x => x.LastName)
                        .ThenBy(x => x.FirstName)
                        .Select(x => new CompanyContactSummary
                        {
                            Id = x.Id,
                            FullName = x.FirstName + " " + x.LastName,
                            JobTitle = x.JobTitle,
                            Email = x.Email
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

        public Task<CompanyRequest?> GetForEditAsync(Guid id) =>
            _db.Companies
                .AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new CompanyRequest
                {
                    Name = c.Name,
                    Industry = c.Industry,
                    Website = c.Website,
                    Phone = c.Phone,
                    Email = c.Email,
                    Address = c.Address,
                    City = c.City,
                    Country = c.Country,
                    Description = c.Description,
                    Status = c.Status
                })
                .FirstOrDefaultAsync();

        public async Task<Guid> CreateAsync(CompanyRequest request, Guid currentUserId)
        {
            var company = new Company
            {
                Id = Guid.NewGuid(),
                CreatedByUserId = currentUserId,
                IsDeleted = false
            };

            Apply(request, company);

            _db.Companies.Add(company);
            await _db.SaveChangesAsync();

            return company.Id;
        }

        public async Task<bool> UpdateAsync(Guid id, CompanyRequest request)
        {
            var existing = await _db.Companies.FirstOrDefaultAsync(c => c.Id == id);
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
            var company = await _db.Companies.FirstOrDefaultAsync(c => c.Id == id);
            if (company is null)
            {
                return ServiceResult.Fail("That company no longer exists.");
            }

            // Block rather than cascade or orphan — the same call made for contacts
            // in Phase 2, now covering deals too. The query filters already exclude
            // soft-deleted rows on both sides.
            var linkedContacts = await _db.Contacts.CountAsync(c => c.CompanyId == id);
            var linkedDeals = await _db.Deals.CountAsync(d => d.CompanyId == id);

            if (linkedContacts > 0 || linkedDeals > 0)
            {
                var blockers = new List<string>();
                if (linkedContacts > 0) blockers.Add($"{linkedContacts} contact(s)");
                if (linkedDeals > 0) blockers.Add($"{linkedDeals} deal(s)");

                return ServiceResult.Fail(
                    $"This company still has {string.Join(" and ", blockers)}. Reassign or delete them first.");
            }

            company.IsDeleted = true;
            await _db.SaveChangesAsync();

            return ServiceResult.Ok();
        }

        public async Task<IReadOnlyList<CompanyOption>> GetOptionsAsync() =>
            await _db.Companies
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new CompanyOption { Id = c.Id, Name = c.Name })
                .ToListAsync();

        /// <summary>Normalising input (trimming) is a service concern, not the controller's.</summary>
        private static void Apply(CompanyRequest request, Company company)
        {
            company.Name = request.Name.Trim();
            company.Industry = Clean(request.Industry);
            company.Website = Clean(request.Website);
            company.Phone = Clean(request.Phone);
            company.Email = Clean(request.Email);
            company.Address = Clean(request.Address);
            company.City = Clean(request.City);
            company.Country = Clean(request.Country);
            company.Description = Clean(request.Description);
            company.Status = request.Status;
        }

        private static string? Clean(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
