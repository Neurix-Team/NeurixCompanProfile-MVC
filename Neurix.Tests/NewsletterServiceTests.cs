using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Neurix.BLL.Dtos;
using Neurix.BLL.Services;
using Neurix.DAL.Data;
using Neurix.DAL.Models;
using Xunit;

namespace Neurix.Tests
{
    public class NewsletterServiceTests : IDisposable
    {
        private readonly CrmDbContext _db;
        private readonly NewsletterService _service;

        public NewsletterServiceTests()
        {
            var options = new DbContextOptionsBuilder<CrmDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _db = new CrmDbContext(options);
            _service = new NewsletterService(_db, NullLogger<NewsletterService>.Instance);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task SubscribeAsync_NewEmail_CreatesActiveSubscription()
        {
            var outcome = await _service.SubscribeAsync("visitor@example.com");

            Assert.Equal(NewsletterSubscribeOutcome.Subscribed, outcome);
            var row = await _db.NewsletterSubscriptions.SingleAsync();
            Assert.Equal("visitor@example.com", row.Email);
            Assert.True(row.IsActive);
            Assert.False(row.IsDeleted);
        }

        [Fact]
        public async Task SubscribeAsync_SameEmailTwice_SecondCallReportsAlreadySubscribed_NoDuplicateRow()
        {
            await _service.SubscribeAsync("visitor@example.com");
            var second = await _service.SubscribeAsync("visitor@example.com");

            Assert.Equal(NewsletterSubscribeOutcome.AlreadySubscribed, second);
            Assert.Single(_db.NewsletterSubscriptions);
        }

        [Fact]
        public async Task SubscribeAsync_IsCaseInsensitiveOnEmail()
        {
            await _service.SubscribeAsync("Visitor@Example.com");
            var second = await _service.SubscribeAsync("visitor@example.com");

            Assert.Equal(NewsletterSubscribeOutcome.AlreadySubscribed, second);
            Assert.Single(_db.NewsletterSubscriptions);
        }

        [Fact]
        public async Task SubscribeAsync_InvalidEmail_ReturnsInvalidEmail_AndStoresNothing()
        {
            var outcome = await _service.SubscribeAsync("not-an-email");

            Assert.Equal(NewsletterSubscribeOutcome.InvalidEmail, outcome);
            Assert.Empty(_db.NewsletterSubscriptions);
        }

        [Fact]
        public async Task SubscribeAsync_ReactivatesASoftDeletedRow_InsteadOfDuplicating()
        {
            _db.NewsletterSubscriptions.Add(new NewsletterSubscription
            {
                Id = Guid.NewGuid(),
                Email = "visitor@example.com",
                IsActive = false,
                IsDeleted = true
            });
            await _db.SaveChangesAsync();

            var outcome = await _service.SubscribeAsync("visitor@example.com");

            Assert.Equal(NewsletterSubscribeOutcome.AlreadySubscribed, outcome);
            var row = await _db.NewsletterSubscriptions.IgnoreQueryFilters().SingleAsync();
            Assert.False(row.IsDeleted);
            Assert.True(row.IsActive);
        }

        [Fact]
        public async Task SubscribeAsync_TrimsWhitespaceAroundEmail()
        {
            await _service.SubscribeAsync("  visitor@example.com  ");

            var row = await _db.NewsletterSubscriptions.SingleAsync();
            Assert.Equal("visitor@example.com", row.Email);
        }
    }
}
