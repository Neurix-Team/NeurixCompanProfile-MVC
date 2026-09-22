namespace Neurix.BLL.Dtos
{
    /// <summary>The signed-in CRM user, as the UI needs to display them.</summary>
    public class CrmUserProfile
    {
        public Guid Id { get; init; }
        public string FullName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();
    }

    /// <summary>Why a profile (display name) update ended the way it did.</summary>
    public enum CrmProfileUpdateOutcome
    {
        Updated,
        NotFound,
        InvalidFullName,
        Unavailable
    }

    /// <summary>Why a password change ended the way it did. Distinct codes keep the UI from lying about the cause.</summary>
    public enum CrmPasswordChangeOutcome
    {
        Success,
        NotFound,

        /// <summary>The supplied current password did not match. Never revealed to other users; only the owner sees it.</summary>
        IncorrectCurrentPassword,

        /// <summary>The new password failed the Identity policy (length, complexity).</summary>
        InvalidNewPassword,
        Unavailable
    }

    /// <summary>
    /// Why a sign-in attempt ended the way it did. The BLL decides this;
    /// the controller only maps it to a redirect or an error message.
    /// </summary>
    public enum CrmSignInOutcome
    {
        Success,
        InvalidCredentials,
        LockedOut,

        /// <summary>
        /// The CRM database could not be reached (missing/invalid connection string,
        /// server asleep, network failure). Distinct from InvalidCredentials so the
        /// operator sees a configuration problem instead of a blank 500 page.
        /// </summary>
        Unavailable
    }
}
