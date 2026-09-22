namespace Neurix.BLL.Dtos
{
    /// <summary>
    /// How a newsletter subscribe attempt ended. "Already subscribed" is a success
    /// state — the address is verified deliverable — so the UI can confirm warmly
    /// without pretending a brand-new signup happened.
    /// </summary>
    public enum NewsletterSubscribeOutcome
    {
        Subscribed,
        AlreadySubscribed,

        /// <summary>Failed the EmailAddress rule — the caller's ModelState should have caught it first.</summary>
        InvalidEmail,

        /// <summary>The CRM database could not be reached; the visitor must be told to retry, not thanked.</summary>
        Unavailable
    }
}
