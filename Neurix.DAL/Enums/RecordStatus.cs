namespace Neurix.DAL.Enums
{
    /// <summary>
    /// Lifecycle status shared by Companies and Contacts.
    /// Pipeline-style statuses (lead stages, deal stages) are semantically
    /// different and get their own enums in later phases.
    /// </summary>
    public enum RecordStatus
    {
        Active = 0,
        Inactive = 1
    }
}
