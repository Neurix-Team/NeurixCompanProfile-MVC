using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neurix.BLL.Common;
using Neurix.BLL.Dtos.Cms;

namespace Neurix.BLL.Services.Cms
{
    public interface ICmsTestimonialService
    {
        Task<IReadOnlyList<CmsTestimonialSummaryDto>> GetTestimonialsByCompanyAsync(Guid? companyProfileId, bool includeUnpublished = false);
        Task<IReadOnlyList<CmsTestimonialSummaryDto>> GetPublishedTestimonialsByCompanySlugAsync(string companySlug);
        Task<IReadOnlyList<CmsTestimonialSummaryDto>> GetFeaturedTestimonialsByCompanySlugAsync(string companySlug);
        Task<CmsTestimonialDetailDto?> GetByIdAsync(Guid id);
        Task<ServiceResult<Guid>> CreateAsync(CmsTestimonialUpsertDto dto, Guid? userId = null);
        Task<ServiceResult> UpdateAsync(Guid id, CmsTestimonialUpsertDto dto, Guid? userId = null);
        Task<ServiceResult> SoftDeleteAsync(Guid id, Guid? userId = null);
        Task<ServiceResult> SetPublishedStatusAsync(Guid id, bool isPublished, Guid? userId = null);
    }
}
