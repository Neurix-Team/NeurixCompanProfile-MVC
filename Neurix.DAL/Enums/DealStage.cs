namespace Neurix.DAL.Enums
{
    /// <summary>
    /// Where a deal sits in the sales pipeline. Its own enum rather than
    /// <see cref="RecordStatus"/> for the same reason <see cref="LeadStatus"/> is:
    /// pipeline stages are workflow states, not an active/inactive flag.
    /// <para>
    /// Unlike <see cref="LeadStatus.Converted"/>, no stage is protected — a deal
    /// produces no side-effect records, so staff move it freely in either direction.
    /// </para>
    /// </summary>
    public enum DealStage
    {
        New = 0,
        Contacted = 1,
        ProposalSent = 2,
        Won = 3,
        Lost = 4
    }
}
