using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Neurix.BLL.Common;
using Neurix.BLL.Dtos.Cms;

namespace Neurix.BLL.Services.Cms
{
    public interface ICmsSocialLinkService
    {
        Task<IReadOnlyList<CmsSocialLinkSummaryDto>> GetLinksByCompanyAsync(Guid? companyProfileId = null, bool includeUnpublished = false, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<CmsSocialLinkSummaryDto>> GetPublishedLinksByCompanySlugAsync(string companySlug, CancellationToken cancellationToken = default);
        Task<CmsSocialLinkDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ServiceResult<Guid>> CreateAsync(CmsSocialLinkUpsertDto dto, Guid? userId = null, CancellationToken cancellationToken = default);
        Task<ServiceResult> UpdateAsync(Guid id, CmsSocialLinkUpsertDto dto, Guid? userId = null, CancellationToken cancellationToken = default);
        Task<ServiceResult> SetPublishedStatusAsync(Guid id, bool isPublished, Guid? userId = null, CancellationToken cancellationToken = default);
        Task<ServiceResult> UpdateDisplayOrderAsync(Guid id, int displayOrder, Guid? userId = null, CancellationToken cancellationToken = default);
        Task<ServiceResult> SoftDeleteAsync(Guid id, Guid? userId = null, CancellationToken cancellationToken = default);
    }
}
