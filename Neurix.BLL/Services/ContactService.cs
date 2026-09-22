using Microsoft.EntityFrameworkCore;
using Neurix.BLL.Common;
using Neurix.BLL.Dtos;
using Neurix.DAL.Data;
using Neurix.DAL.Enums;
using Neurix.DAL.Models;

namespace Neurix.BLL.Services
{
    public class ContactService : IContactService
    {
        private readonly CrmDbContext _db;

        public ContactService(CrmDbContext db)
        {
            _db = db;
        }

        public async Task<PagedResult<ContactListRow>> GetListAsync(
            string? search,
            RecordStatus? status,
            Guid? companyId,
            int page,
            int pageSize = PagingDefaults.DefaultPageSize)
        {
            page = PagingDefaults.ClampPage(page);
            pageSize = PagingDefaults.ClampPageSize(pageSize);

            var query = _db.Contacts.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(c =>
                    c.FirstName.Contains(term) ||
                    c.LastName.Contains(term) ||
                    (c.Email != null && c.Email.Contains(term)) ||
                    (c.Phone != null && c.Phone.Contains(term)) ||
                    (c.Company != null && c.Company.Name.Contains(term)));
            }

            if (status.HasValue)
            {
                query = query.Where(c => c.Status == status.Value);
            }

            if (companyId.HasValue)
            {
                query = query.Where(c => c.CompanyId == companyId.Value);
            }

            var totalCount = await query.CountAsync();

            // Ordering must be deterministic or Skip/Take can repeat or drop rows.
            var items = await query
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .ThenBy(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new ContactListRow
                {
                    Id = c.Id,
                    FullName = c.FirstName + " " + c.LastName,
                    JobTitle = c.JobTitle,
                    Email = c.Email,
                    Phone = c.Phone,
                    CompanyId = c.CompanyId,
                    CompanyName = c.Company != null ? c.Company.Name : null,
                    Status = c.Status
                })
                .ToListAsync();

            return new PagedResult<ContactListRow>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public Task<ContactDetail?> GetDetailAsync(Guid id) =>
            _db.Contacts
                .AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new ContactDetail
                {
                    Id = c.Id,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    FullName = c.FirstName + " " + c.LastName,
                    JobTitle = c.JobTitle,
                    Email = c.Email,
                    Phone = c.Phone,
                    Notes = c.Notes,
                    Status = c.Status,
                    CreatedAtUtc = c.CreatedAtUtc,
                    UpdatedAtUtc = c.UpdatedAtUtc,
                    CompanyId = c.CompanyId,
                    CompanyName = c.Company != null ? c.Company.Name : null,
                    CompanyIndustry = c.Company != null ? c.Company.Industry : null,
                    CompanyEmail = c.Company != null ? c.Company.Email : null,
                    CompanyPhone = c.Company != null ? c.Company.Phone : null
                })
                .FirstOrDefaultAsync();

        public Task<ContactRequest?> GetForEditAsync(Guid id) =>
            _db.Contacts
                .AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new ContactRequest
                {
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    JobTitle = c.JobTitle,
                    Email = c.Email,
                    Phone = c.Phone,
                    Notes = c.Notes,
                    CompanyId = c.CompanyId,
                    Status = c.Status
                })
                .FirstOrDefaultAsync();

        public async Task<Guid> CreateAsync(ContactRequest request, Guid currentUserId)
        {
            var contact = new Contact
            {
                Id = Guid.NewGuid(),
                CreatedByUserId = currentUserId,
                IsDeleted = false
            };

            Apply(request, contact);

            _db.Contacts.Add(contact);
            await _db.SaveChangesAsync();

            return contact.Id;
        }

        public async Task<bool> UpdateAsync(Guid id, ContactRequest request)
        {
            var existing = await _db.Contacts.FirstOrDefaultAsync(c => c.Id == id);
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
            var contact = await _db.Contacts.FirstOrDefaultAsync(c => c.Id == id);
            if (contact is null)
            {
                return ServiceResult.Fail("That contact no longer exists.");
            }

            contact.IsDeleted = true;
            await _db.SaveChangesAsync();

            return ServiceResult.Ok();
        }

        private static void Apply(ContactRequest request, Contact contact)
        {
            contact.FirstName = request.FirstName.Trim();
            contact.LastName = request.LastName.Trim();
            contact.JobTitle = Clean(request.JobTitle);
            contact.Email = Clean(request.Email);
            contact.Phone = Clean(request.Phone);
            contact.Notes = Clean(request.Notes);
            contact.CompanyId = request.CompanyId;
            contact.Status = request.Status;
        }

        private static string? Clean(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
