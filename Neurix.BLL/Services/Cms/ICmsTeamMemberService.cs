using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neurix.BLL.Common;
using Neurix.BLL.Dtos.Cms;

namespace Neurix.BLL.Services.Cms
{
    public interface ICmsTeamMemberService
    {
        Task<IReadOnlyList<CmsTeamMemberSummaryDto>> GetTeamMembersByCompanyAsync(Guid? companyProfileId, bool includeUnpublished = false);
        Task<IReadOnlyList<CmsTeamMemberSummaryDto>> GetPublishedMembersByCompanySlugAsync(string companySlug);
        Task<CmsTeamMemberDetailDto?> GetByIdAsync(Guid id);
        Task<ServiceResult<Guid>> CreateAsync(CmsTeamMemberUpsertDto dto, Guid? userId = null);
        Task<ServiceResult> UpdateAsync(Guid id, CmsTeamMemberUpsertDto dto, Guid? userId = null);
        Task<ServiceResult> SoftDeleteAsync(Guid id, Guid? userId = null);
        Task<ServiceResult> SetPublishedStatusAsync(Guid id, bool isPublished, Guid? userId = null);
    }
}
