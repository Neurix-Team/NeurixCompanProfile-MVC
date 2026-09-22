namespace Neurix.DAL.Enums
{
    /// <summary>
    /// Where a lead sits in the qualification process.
    /// <see cref="Converted"/> is reached only through the dedicated conversion
    /// action — it is not a status staff pick by hand on the edit form.
    /// </summary>
    public enum LeadStatus
    {
        New = 0,
        Contacted = 1,
        Qualified = 2,
        Unqualified = 3,
        Converted = 4
    }
}
