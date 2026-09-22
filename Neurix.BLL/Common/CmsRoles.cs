namespace Neurix.BLL.Common
{
    /// <summary>
    /// Role constants for the CMS module.
    /// Reuses the existing Admin role for full CMS access, with room for future specialized CMS roles.
    /// </summary>
    public static class CmsRoles
    {
        public const string Admin = "Admin";
        public const string CmsEditor = "CmsEditor";

        /// <summary>Roles authorized to access the CMS management area.</summary>
        public const string AdminOrEditor = Admin + "," + CmsEditor;

        /// <summary>Roles authorized for full administrative operations.</summary>
        public const string AdminOnly = Admin;
    }
}
