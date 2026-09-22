using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Neurix.BLL.Common;
using Neurix.BLL.Dtos.Cms;

namespace Neurix.BLL.Services.Cms
{
    public interface ICmsCompanyProfileService
    {
        Task<IReadOnlyList<CmsCompanyProfileSummaryDto>> GetAllProfilesAsync(bool includeUnpublished = false, CancellationToken cancellationToken = default);
        Task<CmsCompanyProfileDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<CmsCompanyProfileDetailDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
        Task<CmsCompanyProfileDetailDto?> GetDefaultPublishedProfileAsync(CancellationToken cancellationToken = default);
        Task<ServiceResult<Guid>> CreateAsync(CmsCompanyProfileUpsertDto dto, Guid? userId = null, CancellationToken cancellationToken = default);
        Task<ServiceResult> UpdateAsync(Guid id, CmsCompanyProfileUpsertDto dto, Guid? userId = null, CancellationToken cancellationToken = default);
        Task<ServiceResult> SetPublishedStatusAsync(Guid id, bool isPublished, Guid? userId = null, CancellationToken cancellationToken = default);
        Task<ServiceResult> SoftDeleteAsync(Guid id, Guid? userId = null, CancellationToken cancellationToken = default);
    }
}
