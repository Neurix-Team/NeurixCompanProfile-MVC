using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neurix.BLL.Common;
using Neurix.BLL.Dtos.Cms;

namespace Neurix.BLL.Services.Cms
{
    public interface ICmsHomeSectionService
    {
        // ── Hero Section ──
        Task<CmsHeroSectionDto?> GetHeroSectionByCompanySlugAsync(string slug, bool includeUnpublished = false);
        Task<CmsHeroSectionDto?> GetHeroSectionByCompanyIdAsync(Guid companyProfileId);
        Task<CmsHeroSectionDto?> GetHeroSectionByIdAsync(Guid id);
        Task<ServiceResult> UpsertHeroSectionAsync(CmsHeroSectionUpsertDto dto, Guid? userId = null);

        // ── Human Vision Section ──
        Task<CmsHumanVisionSectionDto?> GetHumanVisionSectionByCompanySlugAsync(string slug, bool includeUnpublished = false);
        Task<CmsHumanVisionSectionDto?> GetHumanVisionSectionByCompanyIdAsync(Guid companyProfileId);
        Task<CmsHumanVisionSectionDto?> GetHumanVisionSectionByIdAsync(Guid id);
        Task<ServiceResult> UpsertHumanVisionSectionAsync(CmsHumanVisionSectionUpsertDto dto, Guid? userId = null);

        // ── Message to Pioneers Section ──
        Task<CmsPioneersSectionDto?> GetPioneersSectionByCompanySlugAsync(string slug, bool includeUnpublished = false);
        Task<CmsPioneersSectionDto?> GetPioneersSectionByCompanyIdAsync(Guid companyProfileId);
        Task<CmsPioneersSectionDto?> GetPioneersSectionByIdAsync(Guid id);
        Task<ServiceResult> UpsertPioneersSectionAsync(CmsPioneersSectionUpsertDto dto, Guid? userId = null);

        // ── Core Capabilities / Pillars Section (#pillars) ──
        Task<CmsPillarsSectionDto?> GetPillarsSectionByCompanySlugAsync(string slug, bool includeUnpublished = false);
        Task<CmsPillarsSectionDto?> GetPillarsSectionByCompanyIdAsync(Guid companyProfileId);
        Task<ServiceResult> UpsertPillarsSectionAsync(CmsPillarsSectionUpsertDto dto, Guid? userId = null);

        // ── Pillar Items (List) ──
        Task<IReadOnlyList<CmsPillarItemDto>> GetPillarItemsByCompanySlugAsync(string slug, bool includeUnpublished = false);
        Task<IReadOnlyList<CmsPillarItemDto>> GetPillarItemsByCompanyIdAsync(Guid companyProfileId, bool includeUnpublished = true);
        Task<CmsPillarItemDto?> GetPillarItemByIdAsync(Guid id);
        Task<ServiceResult<Guid>> CreatePillarItemAsync(CmsPillarItemUpsertDto dto, Guid? userId = null);
        Task<ServiceResult> UpdatePillarItemAsync(CmsPillarItemUpsertDto dto, Guid? userId = null);
        Task<ServiceResult> DeletePillarItemAsync(Guid id, Guid? userId = null);

        // ── Our Ecosystem / Divisions Section (#divisions) ──
        Task<CmsDivisionsSectionDto?> GetDivisionsSectionByCompanySlugAsync(string slug, bool includeUnpublished = false);
        Task<CmsDivisionsSectionDto?> GetDivisionsSectionByCompanyIdAsync(Guid companyProfileId);
        Task<ServiceResult> UpsertDivisionsSectionAsync(CmsDivisionsSectionUpsertDto dto, Guid? userId = null);

        // ── Division Cards (List) ──
        Task<IReadOnlyList<CmsDivisionItemDto>> GetDivisionItemsByCompanySlugAsync(string slug, bool includeUnpublished = false);
        Task<IReadOnlyList<CmsDivisionItemDto>> GetDivisionItemsByCompanyIdAsync(Guid companyProfileId, bool includeUnpublished = true);
        Task<CmsDivisionItemDto?> GetDivisionItemByIdAsync(Guid id);
        Task<ServiceResult<Guid>> CreateDivisionItemAsync(CmsDivisionItemUpsertDto dto, Guid? userId = null);
        Task<ServiceResult> UpdateDivisionItemAsync(CmsDivisionItemUpsertDto dto, Guid? userId = null);
        Task<ServiceResult> DeleteDivisionItemAsync(Guid id, Guid? userId = null);

        // ── AI & Engineering Section (#services) ──
        Task<CmsAiEngineeringSectionDto?> GetAiEngineeringSectionByCompanySlugAsync(string slug, bool includeUnpublished = false);
        Task<CmsAiEngineeringSectionDto?> GetAiEngineeringSectionByCompanyIdAsync(Guid companyProfileId);
        Task<ServiceResult> UpsertAiEngineeringSectionAsync(CmsAiEngineeringSectionUpsertDto dto, Guid? userId = null);

        // ── AI & Engineering Cards (List) ──
        Task<IReadOnlyList<CmsAiEngineeringItemDto>> GetAiEngineeringItemsByCompanySlugAsync(string slug, bool includeUnpublished = false);
        Task<IReadOnlyList<CmsAiEngineeringItemDto>> GetAiEngineeringItemsByCompanyIdAsync(Guid companyProfileId, bool includeUnpublished = true);
        Task<CmsAiEngineeringItemDto?> GetAiEngineeringItemByIdAsync(Guid id);
        Task<ServiceResult<Guid>> CreateAiEngineeringItemAsync(CmsAiEngineeringItemUpsertDto dto, Guid? userId = null);
        Task<ServiceResult> UpdateAiEngineeringItemAsync(CmsAiEngineeringItemUpsertDto dto, Guid? userId = null);
        Task<ServiceResult> DeleteAiEngineeringItemAsync(Guid id, Guid? userId = null);

        // ── List-Section Headers (#portfolio, #insights, #testimonials) ──
        Task<CmsListSectionHeaderDto?> GetListSectionHeaderByCompanySlugAsync(string slug, string sectionKey, bool includeUnpublished = false);
        Task<CmsListSectionHeaderDto?> GetListSectionHeaderByCompanyIdAsync(Guid companyProfileId, string sectionKey);
        Task<IReadOnlyList<CmsListSectionHeaderDto>> GetListSectionHeadersByCompanyIdAsync(Guid companyProfileId);
        Task<ServiceResult> UpsertListSectionHeaderAsync(CmsListSectionHeaderUpsertDto dto, Guid? userId = null);

        // ── Vision & Implementation Framework Section (#ethics) ──
        Task<CmsEthicsSectionDto?> GetEthicsSectionByCompanySlugAsync(string slug, bool includeUnpublished = false);
        Task<CmsEthicsSectionDto?> GetEthicsSectionByCompanyIdAsync(Guid companyProfileId);
        Task<CmsEthicsSectionDto?> GetEthicsSectionByIdAsync(Guid id);
        Task<ServiceResult> UpsertEthicsSectionAsync(CmsEthicsSectionUpsertDto dto, Guid? userId = null);

        // ── Call To Action Section (#cta) ──
        Task<CmsCtaSectionDto?> GetCtaSectionByCompanySlugAsync(string slug, bool includeUnpublished = false);
        Task<CmsCtaSectionDto?> GetCtaSectionByCompanyIdAsync(Guid companyProfileId);
        Task<CmsCtaSectionDto?> GetCtaSectionByIdAsync(Guid id);
        Task<ServiceResult> UpsertCtaSectionAsync(CmsCtaSectionUpsertDto dto, Guid? userId = null);
    }
}
