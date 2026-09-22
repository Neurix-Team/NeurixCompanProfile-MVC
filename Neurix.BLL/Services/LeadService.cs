using Microsoft.EntityFrameworkCore;
using Neurix.BLL.Common;
using Neurix.BLL.Dtos;
using Neurix.DAL.Data;
using Neurix.DAL.Enums;
using Neurix.DAL.Models;

namespace Neurix.BLL.Services
{
    public class LeadService : ILeadService
    {
        private readonly CrmDbContext _db;

        public LeadService(CrmDbContext db)
        {
            _db = db;
        }

        public async Task<PagedResult<LeadListRow>> GetListAsync(
            string? search,
            LeadStatus? status,
            Guid? assignedToUserId,
            int page,
            int pageSize = PagingDefaults.DefaultPageSize,
            bool unassignedOnly = false)
        {
            page = PagingDefaults.ClampPage(page);
            pageSize = PagingDefaults.ClampPageSize(pageSize);

            var query = _db.Leads.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(l =>
                    l.FullName.Contains(term) ||
                    l.Email.Contains(term) ||
                    (l.CompanyName != null && l.CompanyName.Contains(term)) ||
                    (l.Phone != null && l.Phone.Contains(term)));
            }

            if (status.HasValue)
            {
                query = query.Where(l => l.Status == status.Value);
            }

            if (unassignedOnly)
            {
                query = query.Where(l => l.AssignedToUserId == null);
            }
            else if (assignedToUserId.HasValue)
            {
                query = query.Where(l => l.AssignedToUserId == assignedToUserId.Value);
            }

            var totalCount = await query.CountAsync();

            // Newest first — the natural queue order for triaging leads.
            // The Id tiebreaker keeps Skip/Take stable when timestamps collide.
            var items = await query
                .OrderByDescending(l => l.CreatedAtUtc)
                .ThenBy(l => l.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new LeadListRow
                {
                    Id = l.Id,
                    FullName = l.FullName,
                    Email = l.Email,
                    CompanyName = l.CompanyName,
                    Phone = l.Phone,
                    Status = l.Status,
                    Source = l.Source,
                    AssignedToUserName = l.AssignedToUser != null ? l.AssignedToUser.FullName : null,
                    CreatedAtUtc = l.CreatedAtUtc
                })
                .ToListAsync();

            return new PagedResult<LeadListRow>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public Task<LeadDetail?> GetDetailAsync(Guid id) =>
            _db.Leads
                .AsNoTracking()
                .Where(l => l.Id == id)
                .Select(l => new LeadDetail
                {
                    Id = l.Id,
                    FullName = l.FullName,
                    Email = l.Email,
                    CompanyName = l.CompanyName,
                    Phone = l.Phone,
                    Source = l.Source,
                    Status = l.Status,
                    InquiryType = l.InquiryType,
                    Message = l.Message,
                    Notes = l.Notes,
                    AssignedToUserId = l.AssignedToUserId,
                    AssignedToUserName = l.AssignedToUser != null ? l.AssignedToUser.FullName : null,
                    CreatedAtUtc = l.CreatedAtUtc,
                    UpdatedAtUtc = l.UpdatedAtUtc,
                    CapturedFromWebsiteForm = l.CreatedByUserId == null,
                    ConvertedAtUtc = l.ConvertedAtUtc,
                    ConvertedToCompanyId = l.ConvertedToCompanyId,
                    ConvertedToCompanyName = l.ConvertedToCompany != null ? l.ConvertedToCompany.Name : null,
                    ConvertedToContactId = l.ConvertedToContactId,
                    ConvertedToContactName = l.ConvertedToContact != null
                        ? (l.ConvertedToContact.FirstName + " " + l.ConvertedToContact.LastName).Trim()
                        : null
                })
                .FirstOrDefaultAsync();

        public Task<LeadRequest?> GetForEditAsync(Guid id) =>
            _db.Leads
                .AsNoTracking()
                .Where(l => l.Id == id)
                .Select(l => new LeadRequest
                {
                    FullName = l.FullName,
                    Email = l.Email,
                    CompanyName = l.CompanyName,
                    Phone = l.Phone,
                    Source = l.Source,
                    Status = l.Status,
                    InquiryType = l.InquiryType,
                    Message = l.Message,
                    Notes = l.Notes,
                    AssignedToUserId = l.AssignedToUserId
                })
                .FirstOrDefaultAsync();

        public async Task<Guid> CreateAsync(LeadRequest request, Guid currentUserId)
        {
            var lead = new Lead
            {
                Id = Guid.NewGuid(),
                CreatedByUserId = currentUserId,
                IsDeleted = false
            };

            Apply(request, lead);

            _db.Leads.Add(lead);
            await _db.SaveChangesAsync();

            return lead.Id;
        }

        public async Task<LeadUpdateOutcome> UpdateAsync(Guid id, LeadRequest request)
        {
            var existing = await _db.Leads.FirstOrDefaultAsync(l => l.Id == id);
            if (existing is null)
            {
                return LeadUpdateOutcome.NotFound;
            }

            var alreadyConverted = existing.Status == LeadStatus.Converted;

            // Converted is an outcome, not an opinion. It is only ever set by the
            // conversion service, which also creates the company and contact that
            // the status is claiming exist.
            if (request.Status == LeadStatus.Converted && !alreadyConverted)
            {
                return LeadUpdateOutcome.ConversionNotAllowedHere;
            }

            Apply(request, existing);

            // Converted is equally not something to be edited back out of: the
            // company and contact it produced still exist. The edit form shows the
            // status as read-only in this case, so nothing is being overridden
            // behind the user's back.
            if (alreadyConverted)
            {
                existing.Status = LeadStatus.Converted;
            }

            await _db.SaveChangesAsync();
            return LeadUpdateOutcome.Updated;
        }

        public async Task<ServiceResult> SoftDeleteAsync(Guid id)
        {
            var lead = await _db.Leads.FirstOrDefaultAsync(l => l.Id == id);
            if (lead is null)
            {
                return ServiceResult.Fail("That lead no longer exists.");
            }

            lead.IsDeleted = true;
            await _db.SaveChangesAsync();

            return ServiceResult.Ok();
        }

        public async Task<IReadOnlyList<AssignableUser>> GetAssignableUsersAsync() =>
            await _db.Users
                .AsNoTracking()
                .OrderBy(u => u.FullName)
                .Select(u => new AssignableUser { Id = u.Id, FullName = u.FullName })
                .ToListAsync();

        public async Task<Guid> CaptureWebsiteEnquiryAsync(WebsiteEnquiry enquiry)
        {
            var lead = new Lead
            {
                Id = Guid.NewGuid(),
                FullName = enquiry.FullName?.Trim() ?? string.Empty,
                Email = enquiry.Email?.Trim() ?? string.Empty,
                CompanyName = Clean(enquiry.CompanyName),
                InquiryType = Clean(enquiry.InquiryType),
                Message = Clean(enquiry.Message),

                // Decided here, not by the caller: an anonymous website visitor
                // cannot choose their own source, status, owner, or creator.
                Source = LeadSource.Website,
                Status = LeadStatus.New,
                AssignedToUserId = null,
                CreatedByUserId = null,
                IsDeleted = false
            };

            _db.Leads.Add(lead);
            await _db.SaveChangesAsync();

            return lead.Id;
        }

        private static void Apply(LeadRequest request, Lead lead)
        {
            lead.FullName = request.FullName.Trim();
            lead.Email = request.Email.Trim();
            lead.CompanyName = Clean(request.CompanyName);
            lead.Phone = Clean(request.Phone);
            lead.Source = request.Source;
            lead.Status = request.Status;
            lead.InquiryType = Clean(request.InquiryType);
            lead.Message = Clean(request.Message);
            lead.Notes = Clean(request.Notes);
            lead.AssignedToUserId = request.AssignedToUserId;
        }

        private static string? Clean(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
