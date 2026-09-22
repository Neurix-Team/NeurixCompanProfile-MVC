namespace Neurix.BLL.Common
{
    /// <summary>
    /// The CRM's role names. These live in the BLL because "who is allowed to do
    /// what" is a business rule, not a UI or storage concern. They are compile-time
    /// constants so controllers can use them inside [Authorize] attributes.
    /// </summary>
    public static class CrmRoles
    {
        public const string Admin = "Admin";
        public const string CrmStaff = "CrmStaff";

        /// <summary>Every role allowed into the CRM. Used by [Authorize(Roles = ...)].</summary>
        public const string AdminOrStaff = Admin + "," + CrmStaff;

        public static readonly string[] All = { Admin, CrmStaff };
    }
}
