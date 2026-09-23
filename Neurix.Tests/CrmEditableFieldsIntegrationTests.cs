using Microsoft.EntityFrameworkCore;
using Neurix.BLL.Dtos;
using Neurix.BLL.Services;
using Neurix.DAL.Data;
using Neurix.DAL.Enums;

namespace Neurix.Tests;

public sealed class CrmEditableFieldsIntegrationTests
{
    [Fact]
    public async Task EditsAcrossCompanyContactLeadAndDeal_PersistInTheirDetailViews()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var db = new CrmDbContext(options);
        var userId = Guid.NewGuid();
        var companies = new CompanyService(db);
        var contacts = new ContactService(db);
        var leads = new LeadService(db);
        var deals = new DealService(db);

        var companyId = await companies.CreateAsync(new CompanyRequest { Name = "Original Company" }, userId);
        Assert.True(await companies.UpdateAsync(companyId, new CompanyRequest
        {
            Name = "Updated Company", Industry = "Research", Website = "https://example.test",
            Phone = "+20 100 123 4567", Email = "office@example.test", Address = "98 Hassan Maamoun",
            City = "Cairo", Country = "Egypt", Description = "Updated company description"
        }));

        var contactId = await contacts.CreateAsync(new ContactRequest { FirstName = "Original", LastName = "Person" }, userId);
        Assert.True(await contacts.UpdateAsync(contactId, new ContactRequest
        {
            FirstName = "Maya", LastName = "Darwish", JobTitle = "Editor", Email = "maya@example.test",
            Phone = "+20 111 222 3333", Notes = "Updated contact notes", CompanyId = companyId
        }));

        var leadId = await leads.CreateAsync(new LeadRequest { FullName = "Original Lead", Email = "lead@example.test" }, userId);
        Assert.Equal(LeadUpdateOutcome.Updated, await leads.UpdateAsync(leadId, new LeadRequest
        {
            FullName = "Updated Lead", Email = "newlead@example.test", CompanyName = "Updated Company",
            Phone = "+20 122 333 4444", InquiryType = "Partnership", Message = "Updated enquiry",
            Notes = "Updated lead notes", Source = LeadSource.Website, Status = LeadStatus.Qualified
        }));

        var dealId = await deals.CreateAsync(new DealRequest { Name = "Original Deal", CompanyId = companyId }, userId);
        Assert.True(await deals.UpdateAsync(dealId, new DealRequest
        {
            Name = "Updated Deal", CompanyId = companyId, ContactId = contactId,
            Value = 12500.50m, Stage = DealStage.ProposalSent, Notes = "Updated deal notes"
        }));

        db.ChangeTracker.Clear();
        var company = await companies.GetDetailAsync(companyId);
        Assert.NotNull(company);
        Assert.Equal("Updated Company", company.Name);
        Assert.Equal("Research", company.Industry);
        Assert.Equal("https://example.test", company.Website);
        Assert.Equal("+20 100 123 4567", company.Phone);
        Assert.Equal("office@example.test", company.Email);
        Assert.Equal("98 Hassan Maamoun", company.Address);
        Assert.Equal("Cairo", company.City);
        Assert.Equal("Egypt", company.Country);
        Assert.Equal("Updated company description", company.Description);

        var contact = await contacts.GetDetailAsync(contactId);
        Assert.NotNull(contact);
        Assert.Equal("Maya Darwish", contact.FullName);
        Assert.Equal("Editor", contact.JobTitle);
        Assert.Equal("maya@example.test", contact.Email);
        Assert.Equal("+20 111 222 3333", contact.Phone);
        Assert.Equal("Updated contact notes", contact.Notes);
        Assert.Equal("Updated Company", contact.CompanyName);

        var lead = await leads.GetDetailAsync(leadId);
        Assert.NotNull(lead);
        Assert.Equal("Updated Lead", lead.FullName);
        Assert.Equal("newlead@example.test", lead.Email);
        Assert.Equal("Updated Company", lead.CompanyName);
        Assert.Equal("+20 122 333 4444", lead.Phone);
        Assert.Equal("Partnership", lead.InquiryType);
        Assert.Equal("Updated enquiry", lead.Message);
        Assert.Equal("Updated lead notes", lead.Notes);
        Assert.Equal(LeadStatus.Qualified, lead.Status);

        var deal = await deals.GetDetailAsync(dealId);
        Assert.NotNull(deal);
        Assert.Equal("Updated Deal", deal.Name);
        Assert.Equal("Updated Company", deal.CompanyName);
        Assert.Equal("Maya Darwish", deal.ContactName);
        Assert.Equal(12500.50m, deal.Value);
        Assert.Equal(DealStage.ProposalSent, deal.Stage);
        Assert.Equal("Updated deal notes", deal.Notes);
    }
}
