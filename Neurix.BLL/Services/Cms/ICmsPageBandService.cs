using Neurix.BLL.Common;
using Neurix.BLL.Dtos.Cms;

namespace Neurix.BLL.Services.Cms
{
    /// <summary>
    /// CMS-editable bands on the secondary public pages — the five division pages, the AI
    /// capabilities page, the insights and portfolio list pages, and the About page. The
    /// homepage has its own service (<see cref="ICmsHomeSectionService"/>); this one covers
    /// everything else, keyed by page and band so one implementation serves all of them.
    /// </summary>
    public interface ICmsPageBandService
    {
        /// <summary>
        /// Every published band and item for one page in a single round trip — what the public
        /// pages call. Returns an empty result rather than null when nothing is seeded yet, so
        /// callers can always fall through to their own default copy.
        /// </summary>
        Task<CmsPageContentDto> GetPageContentByCompanySlugAsync(string slug, string pageKey, bool includeUnpublished = false);

        Task<CmsPageBandDto?> GetBandByCompanyIdAsync(Guid companyProfileId, string pageKey, string bandKey);

        /// <summary>All bands for one page, published or not — the dashboard's page editor.</summary>
        Task<IReadOnlyList<CmsPageBandDto>> GetBandsByCompanyIdAsync(Guid companyProfileId, string pageKey);

        /// <summary>Bands across every page for one brand — the dashboard's page list.</summary>
        Task<IReadOnlyList<CmsPageBandDto>> GetAllBandsByCompanyIdAsync(Guid companyProfileId);

        /// <summary>
        /// Creates the band row on first save and updates it in place afterwards. Rejects a
        /// page/band pair that <c>CmsPageKeys</c> does not know, so a bad key cannot create a
        /// row that no page renders.
        /// </summary>
        Task<ServiceResult> UpsertBandAsync(CmsPageBandUpsertDto dto, Guid? userId = null);

        Task<IReadOnlyList<CmsPageBandItemDto>> GetBandItemsByCompanyIdAsync(Guid companyProfileId, string pageKey, string bandKey);

        /// <summary>
        /// Every item across every page for one brand, so the dashboard overview can show a
        /// count per band without a query per band.
        /// </summary>
        Task<IReadOnlyList<CmsPageBandItemDto>> GetAllBandItemsByCompanyIdAsync(Guid companyProfileId);

        Task<CmsPageBandItemDto?> GetBandItemByIdAsync(Guid id);

        Task<ServiceResult<Guid>> CreateBandItemAsync(CmsPageBandItemUpsertDto dto, Guid? userId = null);

        Task<ServiceResult> UpdateBandItemAsync(Guid id, CmsPageBandItemUpsertDto dto, Guid? userId = null);

        /// <summary>Soft delete — the row stays for audit and is filtered out by the query filter.</summary>
        Task<ServiceResult> DeleteBandItemAsync(Guid id);
    }
}
