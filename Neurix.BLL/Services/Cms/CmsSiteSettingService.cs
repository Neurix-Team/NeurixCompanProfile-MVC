using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Neurix.BLL.Common;
using Neurix.BLL.Dtos.Cms;
using Neurix.DAL.Data;
using Neurix.DAL.Models;

namespace Neurix.BLL.Services.Cms
{
    public class CmsSiteSettingService : ICmsSiteSettingService
    {
        private readonly CmsDbContext _db;

        private static readonly Regex KeyPattern = new(@"^[a-z0-9]+(\.[a-z0-9]+)*$", RegexOptions.Compiled);
        private static readonly HashSet<string> AllowedSettingTypes = new(StringComparer.OrdinalIgnoreCase)
            { "text", "textarea", "html", "url", "boolean" };
        private static readonly HashSet<string> AllowedGroupNames = new(StringComparer.OrdinalIgnoreCase)
            { "General", "SEO", "Contact", "Footer" };

        public CmsSiteSettingService(CmsDbContext db)
        {
            _db = db;
        }

        private static ServiceResult<string> NormalizeAndValidateKey(string? key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return ServiceResult<string>.Fail("Setting key is required.");
            }

            var normalized = key.Trim().ToLowerInvariant();
            if (!KeyPattern.IsMatch(normalized))
            {
                return ServiceResult<string>.Fail("Key must be dot-separated lowercase alphanumeric segments (e.g. 'seo.og.title').");
            }

            return ServiceResult<string>.Ok(normalized);
        }

        public async Task<IReadOnlyList<CmsSiteSettingDto>> GetSettingsByCompanyAsync(Guid? companyProfileId)
        {
            var query = _db.SiteSettings
                .Include(s => s.CompanyProfile)
                .AsNoTracking();

            if (companyProfileId.HasValue && companyProfileId.Value != Guid.Empty)
            {
                query = query.Where(s => s.CompanyProfileId == companyProfileId.Value);
            }

            return await query
                .OrderBy(s => s.GroupName)
                .ThenBy(s => s.Key)
                .Select(s => new CmsSiteSettingDto
                {
                    Id = s.Id,
                    CompanyProfileId = s.CompanyProfileId,
                    CompanyNameEn = s.CompanyProfile != null ? s.CompanyProfile.NameEn : string.Empty,
                    Key = s.Key,
                    ValueEn = s.ValueEn,
                    ValueAr = s.ValueAr,
                    SettingType = s.SettingType,
                    GroupName = s.GroupName,
                    Label = s.Label,
                    UpdatedAtUtc = s.UpdatedAtUtc ?? s.CreatedAtUtc
                })
                .ToListAsync();
        }

        public async Task<IReadOnlyList<CmsSiteSettingDto>> GetSettingsByCompanySlugAsync(string companySlug)
        {
            if (string.IsNullOrWhiteSpace(companySlug))
                return Array.Empty<CmsSiteSettingDto>();

            var normalized = companySlug.Trim().ToLowerInvariant();

            return await _db.SiteSettings
                .Include(s => s.CompanyProfile)
                .AsNoTracking()
                .Where(s => s.CompanyProfile != null && s.CompanyProfile.Slug == normalized && s.CompanyProfile.IsPublished)
                .OrderBy(s => s.GroupName)
                .ThenBy(s => s.Key)
                .Select(s => new CmsSiteSettingDto
                {
                    Id = s.Id,
                    CompanyProfileId = s.CompanyProfileId,
                    CompanyNameEn = s.CompanyProfile != null ? s.CompanyProfile.NameEn : string.Empty,
                    Key = s.Key,
                    ValueEn = s.ValueEn,
                    ValueAr = s.ValueAr,
                    SettingType = s.SettingType,
                    GroupName = s.GroupName,
                    Label = s.Label,
                    UpdatedAtUtc = s.UpdatedAtUtc ?? s.CreatedAtUtc
                })
                .ToListAsync();
        }

        public async Task<CmsSiteSettingDto?> GetSettingAsync(string companySlug, string key)
        {
            if (string.IsNullOrWhiteSpace(companySlug) || string.IsNullOrWhiteSpace(key))
                return null;

            var normalizedSlug = companySlug.Trim().ToLowerInvariant();
            var normalizedKey = key.Trim().ToLowerInvariant();

            var setting = await _db.SiteSettings
                .Include(s => s.CompanyProfile)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.CompanyProfile != null &&
                                          s.CompanyProfile.Slug == normalizedSlug &&
                                          s.Key.ToLower() == normalizedKey);

            if (setting == null) return null;

            return new CmsSiteSettingDto
            {
                Id = setting.Id,
                CompanyProfileId = setting.CompanyProfileId,
                CompanyNameEn = setting.CompanyProfile != null ? setting.CompanyProfile.NameEn : string.Empty,
                Key = setting.Key,
                ValueEn = setting.ValueEn,
                ValueAr = setting.ValueAr,
                SettingType = setting.SettingType,
                GroupName = setting.GroupName,
                Label = setting.Label,
                UpdatedAtUtc = setting.UpdatedAtUtc ?? setting.CreatedAtUtc
            };
        }

        private static ServiceResult<string> ValidateSettingDto(CmsSiteSettingUpsertDto? dto)
        {
            if (dto == null)
                return ServiceResult<string>.Fail("Setting data is required.");

            var keyResult = NormalizeAndValidateKey(dto.Key);
            if (!keyResult.Success)
                return keyResult;

            if (!AllowedSettingTypes.Contains(dto.SettingType))
                return ServiceResult<string>.Fail($"Invalid setting type '{dto.SettingType}'. Allowed: text, textarea, html, url, boolean.");

            if (!AllowedGroupNames.Contains(dto.GroupName))
                return ServiceResult<string>.Fail($"Invalid group name '{dto.GroupName}'. Allowed: General, SEO, Contact, Footer.");

            // Carries the normalized key forward to the caller.
            return keyResult;
        }

        public async Task<string?> GetSettingValueEnAsync(string companySlug, string key, string? defaultValue = null)
        {
            var setting = await GetSettingAsync(companySlug, key);
            return !string.IsNullOrWhiteSpace(setting?.ValueEn) ? setting.ValueEn : defaultValue;
        }

        public async Task<ServiceResult<Guid>> UpsertAsync(CmsSiteSettingUpsertDto dto, Guid? userId = null)
        {
            var validation = ValidateSettingDto(dto);
            if (!validation.Success)
                return ServiceResult<Guid>.Fail(validation.ErrorMessage!);

            var key = validation.Value!;

            var existing = await _db.SiteSettings
                .FirstOrDefaultAsync(s => s.CompanyProfileId == dto.CompanyProfileId && s.Key == key);

            if (existing != null)
            {
                existing.ValueEn = dto.ValueEn;
                existing.ValueAr = dto.ValueAr;
                existing.SettingType = dto.SettingType;
                existing.GroupName = dto.GroupName;
                existing.Label = dto.Label;
                existing.CreatedByUserId = userId ?? existing.CreatedByUserId;

                await _db.SaveChangesAsync();
                return ServiceResult<Guid>.Ok(existing.Id);
            }

            var newSetting = new CmsSiteSetting
            {
                Id = Guid.NewGuid(),
                CompanyProfileId = dto.CompanyProfileId,
                Key = key,
                ValueEn = dto.ValueEn,
                ValueAr = dto.ValueAr,
                SettingType = dto.SettingType,
                GroupName = dto.GroupName,
                Label = dto.Label,
                CreatedByUserId = userId
            };

            _db.SiteSettings.Add(newSetting);
            await _db.SaveChangesAsync();
            return ServiceResult<Guid>.Ok(newSetting.Id);
        }

        public async Task<ServiceResult> SaveBatchAsync(Guid companyProfileId, IEnumerable<CmsSiteSettingUpsertDto> settings, Guid? userId = null)
        {
            if (settings == null)
                return ServiceResult.Fail("Settings batch is empty.");

            var errors = new List<string>();

            foreach (var item in settings)
            {
                item.CompanyProfileId = companyProfileId;
                var result = await UpsertAsync(item, userId);
                if (!result.Success)
                {
                    var keyDesc = string.IsNullOrWhiteSpace(item.Key) ? "(empty key)" : item.Key;
                    errors.Add($"Key '{keyDesc}': {result.ErrorMessage}");
                }
            }

            if (errors.Count > 0)
            {
                return ServiceResult.Fail($"Failed to save settings: {string.Join("; ", errors)}");
            }

            return ServiceResult.Ok();
        }

        public async Task<ServiceResult<Guid>> CreateAsync(CmsSiteSettingUpsertDto dto, Guid? userId = null)
        {
            var validation = ValidateSettingDto(dto);
            if (!validation.Success)
                return ServiceResult<Guid>.Fail(validation.ErrorMessage!);

            var key = validation.Value!;

            var exists = await _db.SiteSettings
                .AnyAsync(s => s.CompanyProfileId == dto.CompanyProfileId && s.Key == key);

            if (exists)
                return ServiceResult<Guid>.Fail($"A setting with key '{key}' already exists for this company profile.");

            var newSetting = new CmsSiteSetting
            {
                Id = Guid.NewGuid(),
                CompanyProfileId = dto.CompanyProfileId,
                Key = key,
                ValueEn = dto.ValueEn,
                ValueAr = dto.ValueAr,
                SettingType = dto.SettingType,
                GroupName = dto.GroupName,
                Label = dto.Label,
                CreatedByUserId = userId
            };

            _db.SiteSettings.Add(newSetting);
            await _db.SaveChangesAsync();
            return ServiceResult<Guid>.Ok(newSetting.Id);
        }

        public async Task<ServiceResult> SoftDeleteAsync(Guid id, Guid? userId = null)
        {
            // FirstOrDefaultAsync, not FindAsync: Find consults the change tracker first and
            // returns an already-tracked entity without querying, which bypasses the global
            // !IsDeleted filter and lets an already-deleted setting report a second success.
            var setting = await _db.SiteSettings.FirstOrDefaultAsync(s => s.Id == id);
            if (setting == null)
                return ServiceResult.Fail("Site setting not found.");

            setting.IsDeleted = true;
            setting.CreatedByUserId = userId ?? setting.CreatedByUserId;
            await _db.SaveChangesAsync();
            return ServiceResult.Ok();
        }
    }
}
