using Neurix.BLL.Common;
using Neurix.BLL.Dtos;

namespace Neurix.BLL.Services
{
    /// <summary>
    /// Turns a lead into a real Company/Contact pair.
    /// <para>
    /// Kept separate from <see cref="ILeadService"/> on purpose: conversion is not
    /// lead CRUD. It writes across three entity sets in one unit of work and owns
    /// its own matching rules, so it earns its own contract rather than bloating
    /// the lead service with a method that behaves nothing like the others.
    /// </para>
    /// </summary>
    public interface ILeadConversionService
    {
        /// <summary>
        /// The confirmation screen's data. Null when the lead does not exist or has
        /// already been converted — there is nothing to confirm in either case.
        /// </summary>
        Task<LeadConversionPreview?> GetPreviewAsync(Guid leadId);

        /// <summary>
        /// Converts the lead. <paramref name="selectedCompanyId"/> null means "use the
        /// default": create a company from the lead's company name, or attach no
        /// company at all when the lead has no company name.
        /// </summary>
        Task<ServiceResult<LeadConversionResult>> ConvertAsync(
            Guid leadId,
            Guid? selectedCompanyId,
            Guid currentUserId);
    }
}
