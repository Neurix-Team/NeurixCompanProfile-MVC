using Microsoft.EntityFrameworkCore;
using Neurix.BLL.Common;
using Neurix.BLL.Dtos;
using Neurix.DAL.Data;
using Neurix.DAL.Enums;
using Neurix.DAL.Models;

namespace Neurix.BLL.Services
{
    public class LeadConversionService : ILeadConversionService
    {
        private readonly CrmDbContext _db;

        public LeadConversionService(CrmDbContext db)
        {
            _db = db;
        }

        public async Task<LeadConversionPreview?> GetPreviewAsync(Guid leadId)
        {
            var lead = await _db.Leads
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == leadId);

            if (lead is null || lead.Status == LeadStatus.Converted)
            {
                return null;
            }

            var companyName = Clean(lead.CompanyName);
            var email = lead.Email.Trim();

            // Both lookups rely on SQL Server's default case-insensitive collation,
            // which is how search and filtering already behave everywhere else in
            // the CRM. Using ToLower() here would only cost the index.
            var suggestedCompanyId = companyName is null
                ? null
                : await _db.Companies
                    .Where(c => c.Name == companyName)
                    .OrderBy(c => c.Id)
                    .Select(c => (Guid?)c.Id)
                    .FirstOrDefaultAsync();

            var existingContact = await _db.Contacts
                .AsNoTracking()
                .Where(c => c.Email == email)
                .OrderBy(c => c.Id)
                .Select(c => new { c.Id, c.FirstName, c.LastName })
                .FirstOrDefaultAsync();

            var companies = await _db.Companies
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ThenBy(c => c.Id)
                .Select(c => new CompanyOption { Id = c.Id, Name = c.Name })
                .ToListAsync();

            return new LeadConversionPreview
            {
                LeadId = lead.Id,
                FullName = lead.FullName,
                Email = lead.Email,
                Phone = lead.Phone,
                CompanyName = companyName,
                SuggestedCompanyId = suggestedCompanyId,
                ExistingContactId = existingContact?.Id,
                ExistingContactName = existingContact is null
                    ? null
                    : $"{existingContact.FirstName} {existingContact.LastName}".Trim(),
                Companies = companies
            };
        }

        public async Task<ServiceResult<LeadConversionResult>> ConvertAsync(
            Guid leadId,
            Guid? selectedCompanyId,
            Guid currentUserId)
        {
            var lead = await _db.Leads.FirstOrDefaultAsync(l => l.Id == leadId);
            if (lead is null)
            {
                return ServiceResult<LeadConversionResult>.Fail("That lead no longer exists.");
            }

            // Re-checked here, not just on the preview: this is what stops a
            // double-submit or a stale tab from creating a second Company/Contact.
            if (lead.Status == LeadStatus.Converted)
            {
                return ServiceResult<LeadConversionResult>.Fail("This lead has already been converted.");
            }

            var company = await ResolveCompanyAsync(lead, selectedCompanyId, currentUserId);
            if (!company.Success)
            {
                return ServiceResult<LeadConversionResult>.Fail(company.ErrorMessage!);
            }

            var contact = await ResolveContactAsync(lead, company.Value, currentUserId);

            lead.Status = LeadStatus.Converted;
            lead.ConvertedAtUtc = DateTime.UtcNow;
            lead.ConvertedToCompanyId = company.Value?.Id;
            lead.ConvertedToContactId = contact.Id;

            // One save for all three entities. The change tracker already batches
            // them into a single transaction, so no explicit BeginTransaction is
            // needed to make the conversion all-or-nothing.
            await _db.SaveChangesAsync();

            return ServiceResult<LeadConversionResult>.Ok(new LeadConversionResult
            {
                CompanyId = company.Value?.Id,
                CompanyName = company.Value?.Name,
                ContactId = contact.Id,
                ContactName = contact.FullName
            });
        }

        /// <summary>
        /// Picks the company the contact should belong to: the one staff chose, a new
        /// one built from the lead's company name, or none when the lead never named
        /// a company. Newly created companies are not saved yet — the caller commits.
        /// </summary>
        private async Task<ServiceResult<Company?>> ResolveCompanyAsync(
            Lead lead,
            Guid? selectedCompanyId,
            Guid currentUserId)
        {
            if (selectedCompanyId.HasValue)
            {
                var existing = await _db.Companies
                    .FirstOrDefaultAsync(c => c.Id == selectedCompanyId.Value);

                return existing is null
                    ? ServiceResult<Company?>.Fail("The company you picked no longer exists.")
                    : ServiceResult<Company?>.Ok(existing);
            }

            var companyName = Clean(lead.CompanyName);
            if (companyName is null)
            {
                return ServiceResult<Company?>.Ok(null);
            }

            var created = new Company
            {
                Id = Guid.NewGuid(),
                Name = companyName,
                Status = RecordStatus.Active,
                CreatedByUserId = currentUserId,
                IsDeleted = false
            };

            _db.Companies.Add(created);
            return ServiceResult<Company?>.Ok(created);
        }

        /// <summary>
        /// Reuses the contact with this email if there is one, otherwise builds a new
        /// one. An existing contact's company link is only filled in when it is empty
        /// — converting a lead must never silently move someone to another company.
        /// </summary>
        private async Task<Contact> ResolveContactAsync(Lead lead, Company? company, Guid currentUserId)
        {
            var email = lead.Email.Trim();

            var existing = await _db.Contacts.FirstOrDefaultAsync(c => c.Email == email);
            if (existing is not null)
            {
                if (existing.CompanyId is null && company is not null)
                {
                    existing.CompanyId = company.Id;
                }

                return existing;
            }

            var (firstName, lastName) = SplitName(lead.FullName);

            var created = new Contact
            {
                Id = Guid.NewGuid(),
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Phone = Clean(lead.Phone),
                CompanyId = company?.Id,
                Status = RecordStatus.Active,
                CreatedByUserId = currentUserId,
                IsDeleted = false
            };

            _db.Contacts.Add(created);
            return created;
        }

        /// <summary>
        /// Splits a lead's single free-text name into the first/last pair a Contact
        /// needs: last whitespace-separated token becomes the surname. Deliberately
        /// naive — no name-parsing library for an MVP, and staff can correct it on
        /// the contact's edit form when it guesses wrong.
        /// </summary>
        private static (string FirstName, string LastName) SplitName(string fullName)
        {
            var trimmed = fullName.Trim();
            var lastSpace = trimmed.LastIndexOf(' ');

            if (lastSpace <= 0)
            {
                return (Truncate(trimmed, 100), string.Empty);
            }

            var first = trimmed[..lastSpace].Trim();
            var last = trimmed[(lastSpace + 1)..].Trim();

            return (Truncate(first, 100), Truncate(last, 100));
        }

        // Lead.FullName allows 200 characters, Contact.FirstName/LastName only 100.
        private static string Truncate(string value, int maxLength) =>
            value.Length <= maxLength ? value : value[..maxLength];

        private static string? Clean(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
