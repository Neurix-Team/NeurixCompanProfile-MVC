using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Neurix.BLL.Common;
using Neurix.BLL.Dtos.Cms;

namespace Neurix.BLL.Services.Cms
{
    public interface ICmsServiceService
    {
        Task<IReadOnlyList<CmsServiceSummaryDto>> GetServicesByCompanyAsync(Guid? companyProfileId = null, bool includeUnpublished = false, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<CmsServiceSummaryDto>> GetPublishedServicesByCompanySlugAsync(string companySlug, CancellationToken cancellationToken = default);
        Task<CmsServiceDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<CmsServiceDetailDto?> GetBySlugAsync(Guid companyProfileId, string slug, CancellationToken cancellationToken = default);
        Task<ServiceResult<Guid>> CreateAsync(CmsServiceUpsertDto dto, Guid? userId = null, CancellationToken cancellationToken = default);
        Task<ServiceResult> UpdateAsync(Guid id, CmsServiceUpsertDto dto, Guid? userId = null, CancellationToken cancellationToken = default);
        Task<ServiceResult> SetPublishedStatusAsync(Guid id, bool isPublished, Guid? userId = null, CancellationToken cancellationToken = default);
        Task<ServiceResult> UpdateDisplayOrderAsync(Guid id, int displayOrder, Guid? userId = null, CancellationToken cancellationToken = default);
        Task<ServiceResult> SoftDeleteAsync(Guid id, Guid? userId = null, CancellationToken cancellationToken = default);
    }
}
