using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neurix.BLL.Common;
using Neurix.BLL.Dtos.Cms;

namespace Neurix.BLL.Services.Cms
{
    public interface ICmsBlogPostService
    {
        Task<IReadOnlyList<CmsBlogPostSummaryDto>> GetBlogPostsByCompanyAsync(Guid? companyProfileId, bool includeUnpublished = false);
        Task<IReadOnlyList<CmsBlogPostSummaryDto>> GetPublishedPostsByCompanySlugAsync(string companySlug, int? limit = null);
        Task<IReadOnlyList<CmsBlogPostSummaryDto>> GetFeaturedPostsByCompanySlugAsync(string companySlug, int? limit = null);
        Task<CmsBlogPostDetailDto?> GetByIdAsync(Guid id);
        Task<CmsBlogPostDetailDto?> GetBySlugAsync(string companySlug, string postSlug);
        Task<ServiceResult<Guid>> CreateAsync(CmsBlogPostUpsertDto dto, Guid? userId = null);
        Task<ServiceResult> UpdateAsync(Guid id, CmsBlogPostUpsertDto dto, Guid? userId = null);
        Task<ServiceResult> SoftDeleteAsync(Guid id, Guid? userId = null);
        Task<ServiceResult> SetPublishedStatusAsync(Guid id, bool isPublished, Guid? userId = null);
    }
}
