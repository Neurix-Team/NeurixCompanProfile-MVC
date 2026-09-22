using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Neurix.BLL.Dtos;
using Neurix.BLL.Services;
using Neurix.DAL.Data;
using Neurix.DAL.Enums;
using Neurix.DAL.Models;
using Xunit;

namespace Neurix.Tests
{
    /// <summary>
    /// NQ-034: the reception-queue filter. Leads captured from the public form have no
    /// owner, so "Unassigned" must be a first-class, combinable list filter — every
    /// captured lead stays discoverable until someone claims it.
    /// </summary>
    public class LeadListFilterTests : IDisposable
    {
        private readonly CrmDbContext _db;
        private readonly LeadService _service;
        private readonly Guid _assigneeId = Guid.NewGuid();

        public LeadListFilterTests()
        {
            var options = new DbContextOptionsBuilder<CrmDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _db = new CrmDbContext(options);
            _service = new LeadService(_db);

            Seed();
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        private void Seed()
        {
            _db.Leads.AddRange(
                new Lead
                {
                    Id = Guid.NewGuid(),
                    FullName = "Unassigned One",
                    Email = "one@example.com",
                    Status = LeadStatus.New,
                    Source = LeadSource.Website,
                    AssignedToUserId = null,
                    CreatedAtUtc = DateTime.UtcNow.AddDays(-3)
                },
                new Lead
                {
                    Id = Guid.NewGuid(),
                    FullName = "Unassigned Two",
                    Email = "two@example.com",
                    Status = LeadStatus.Qualified,
                    Source = LeadSource.Website,
                    AssignedToUserId = null,
                    CreatedAtUtc = DateTime.UtcNow.AddDays(-2)
                },
                new Lead
                {
                    Id = Guid.NewGuid(),
                    FullName = "Owned Lead",
                    Email = "owned@example.com",
                    Status = LeadStatus.New,
                    Source = LeadSource.Website,
                    AssignedToUserId = _assigneeId,
                    CreatedAtUtc = DateTime.UtcNow.AddDays(-1)
                },
                new Lead
                {
                    Id = Guid.NewGuid(),
                    FullName = "Deleted Unassigned",
                    Email = "deleted@example.com",
                    Status = LeadStatus.New,
                    Source = LeadSource.Website,
                    AssignedToUserId = null,
                    IsDeleted = true,
                    CreatedAtUtc = DateTime.UtcNow
                });

            _db.SaveChanges();
        }

        [Fact]
        public async Task UnassignedOnly_ReturnsOnlyOwnerlessNonDeletedLeads()
        {
            var result = await _service.GetListAsync(null, null, null, 1, unassignedOnly: true);

            Assert.Equal(2, result.TotalCount);
            Assert.All(result.Items, row => Assert.Null(row.AssignedToUserName));
            Assert.Contains(result.Items, r => r.Email == "one@example.com");
            Assert.Contains(result.Items, r => r.Email == "two@example.com");
        }

        [Fact]
        public async Task UnassignedOnly_CombinesWithStatusFilter()
        {
            var result = await _service.GetListAsync(null, LeadStatus.Qualified, null, 1, unassignedOnly: true);

            Assert.Single(result.Items);
            Assert.Equal("two@example.com", result.Items[0].Email);
        }

        [Fact]
        public async Task UnassignedOnly_CombinesWithSearch()
        {
            var result = await _service.GetListAsync("Unassigned Two", null, null, 1, unassignedOnly: true);

            Assert.Single(result.Items);
            Assert.Equal("two@example.com", result.Items[0].Email);
        }

        [Fact]
        public async Task WithoutFilter_AllNonDeletedLeadsAppear()
        {
            var result = await _service.GetListAsync(null, null, null, 1);

            Assert.Equal(3, result.TotalCount);
            Assert.Contains(result.Items, r => r.Email == "owned@example.com");
        }

        [Fact]
        public async Task AssigneeFilter_StillWorks()
        {
            var result = await _service.GetListAsync(null, null, _assigneeId, 1);

            Assert.Single(result.Items);
            Assert.Equal("owned@example.com", result.Items[0].Email);
        }
    }
}
