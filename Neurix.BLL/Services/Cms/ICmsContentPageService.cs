using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neurix.BLL.Common;
using Neurix.BLL.Dtos.Cms;

namespace Neurix.BLL.Services.Cms
{
    public interface ICmsContentPageService
    {
        /// <summary>
        /// Published page for public rendering. Returns null when the page, or its brand
        /// profile, is unpublished or missing — callers fall back to their built-in copy.
        /// </summary>
        Task<CmsContentPageDetailDto?> GetBySlugAsync(string companySlug, string pageSlug);

        /// <summary>All pages for the CMS list, including unpublished ones.</summary>
        Task<IReadOnlyList<CmsContentPageSummaryDto>> GetAllByCompanyAsync(Guid? companyProfileId);

        Task<CmsContentPageDetailDto?> GetByIdAsync(Guid id);

        /// <summary>
        /// Creates or updates the page identified by (CompanyProfileId, Slug).
        /// Returns the row id on success.
        /// </summary>
        Task<ServiceResult<Guid>> UpsertAsync(CmsContentPageUpsertDto dto, Guid? userId = null);
    }
}
