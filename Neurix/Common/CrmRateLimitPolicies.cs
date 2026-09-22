namespace Neurix.Common
{
    /// <summary>Named rate-limiting policies registered in Program.cs and applied via [EnableRateLimiting].</summary>
    public static class CrmRateLimitPolicies
    {
        public const string Login = "crm-login";
        public const string Newsletter = "newsletter-subscribe";
    }
}
