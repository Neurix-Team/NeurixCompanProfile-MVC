using Neurix.BLL.Dtos;

namespace Neurix.BLL.Services
{
    /// <summary>
    /// Footer newsletter subscriptions. Kept in the CRM database next to Leads so
    /// marketing data has one home and one backup story.
    /// </summary>
    public interface INewsletterService
    {
        Task<NewsletterSubscribeOutcome> SubscribeAsync(string email);
    }
}
