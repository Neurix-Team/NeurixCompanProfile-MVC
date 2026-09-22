using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Neurix.BLL.Dtos;
using Neurix.DAL.Data;
using Neurix.DAL.Models;

namespace Neurix.BLL.Services
{
    public class NewsletterService : INewsletterService
    {
        private readonly CrmDbContext _db;
        private readonly ILogger<NewsletterService> _logger;

        public NewsletterService(CrmDbContext db, ILogger<NewsletterService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<NewsletterSubscribeOutcome> SubscribeAsync(string email)
        {
            var normalized = email.Trim();

            if (!new EmailAddressAttribute().IsValid(normalized))
            {
                return NewsletterSubscribeOutcome.InvalidEmail;
            }

            // Lowercased for matching, not stored as a different spelling than typed:
            // addresses are case-insensitive for routing but kept verbatim for the owner.
            var matchKey = normalized.ToLowerInvariant();

            try
            {
                // Ignore the soft-delete filter: a previously removed subscription
                // must be reactivated by the same unique key, not duplicated.
                var existing = await _db.NewsletterSubscriptions
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(s => s.Email.ToLower() == matchKey);

                if (existing is not null)
                {
                    if (existing.IsDeleted)
                    {
                        existing.IsDeleted = false;
                        existing.IsActive = true;
                        existing.UpdatedAtUtc = DateTime.UtcNow;
                    }
                    else if (!existing.IsActive)
                    {
                        existing.IsActive = true;
                        existing.UpdatedAtUtc = DateTime.UtcNow;
                    }
                    else
                    {
                        return NewsletterSubscribeOutcome.AlreadySubscribed;
                    }

                    await _db.SaveChangesAsync();
                    return NewsletterSubscribeOutcome.AlreadySubscribed;
                }

                _db.NewsletterSubscriptions.Add(new NewsletterSubscription
                {
                    Id = Guid.NewGuid(),
                    Email = normalized,
                    IsActive = true,
                    IsDeleted = false
                });

                await _db.SaveChangesAsync();
                return NewsletterSubscribeOutcome.Subscribed;
            }
            catch (Exception ex) when (ex is InvalidOperationException or DbUpdateException)
            {
                _logger.LogError(ex, "Failed to store the newsletter subscription for {Email}.", normalized);
                return NewsletterSubscribeOutcome.Unavailable;
            }
        }
    }
}
