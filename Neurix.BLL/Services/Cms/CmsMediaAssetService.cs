using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Neurix.BLL.Common;
using Neurix.BLL.Dtos.Cms;
using Neurix.DAL.Data;
using Neurix.DAL.Models;

namespace Neurix.BLL.Services.Cms
{
    public class CmsMediaAssetService : ICmsMediaAssetService
    {
        private readonly CmsDbContext _db;

        public CmsMediaAssetService(CmsDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<CmsMediaAssetDto>> GetAssetsByCompanyAsync(Guid? companyProfileId, string? category = null)
        {
            var query = _db.MediaAssets
                .Include(a => a.CompanyProfile)
                .AsNoTracking();

            if (companyProfileId.HasValue && companyProfileId.Value != Guid.Empty)
            {
                query = query.Where(a => a.CompanyProfileId == companyProfileId.Value);
            }

            if (!string.IsNullOrWhiteSpace(category) && !category.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                var cat = category.Trim().ToLowerInvariant();
                query = query.Where(a => a.Category != null && a.Category.ToLower() == cat);
            }

            return await query
                .OrderByDescending(a => a.CreatedAtUtc)
                .Select(a => new CmsMediaAssetDto
                {
                    Id = a.Id,
                    CompanyProfileId = a.CompanyProfileId,
                    CompanyNameEn = a.CompanyProfile != null ? a.CompanyProfile.NameEn : string.Empty,
                    FileName = a.FileName,
                    OriginalFileName = a.OriginalFileName,
                    FilePath = a.FilePath,
                    FileSizeBytes = a.FileSizeBytes,
                    ContentType = a.ContentType,
                    AltTextEn = a.AltTextEn,
                    AltTextAr = a.AltTextAr,
                    Category = a.Category,
                    Tags = a.Tags,
                    CreatedAtUtc = a.CreatedAtUtc
                })
                .ToListAsync();
        }

        public async Task<CmsMediaAssetDto?> GetByIdAsync(Guid id)
        {
            var a = await _db.MediaAssets
                .Include(item => item.CompanyProfile)
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == id);

            if (a == null) return null;

            return new CmsMediaAssetDto
            {
                Id = a.Id,
                CompanyProfileId = a.CompanyProfileId,
                CompanyNameEn = a.CompanyProfile != null ? a.CompanyProfile.NameEn : string.Empty,
                FileName = a.FileName,
                OriginalFileName = a.OriginalFileName,
                FilePath = a.FilePath,
                FileSizeBytes = a.FileSizeBytes,
                ContentType = a.ContentType,
                AltTextEn = a.AltTextEn,
                AltTextAr = a.AltTextAr,
                Category = a.Category,
                Tags = a.Tags,
                CreatedAtUtc = a.CreatedAtUtc
            };
        }

        public async Task<IReadOnlyList<CmsMediaAssetUsageDto>> GetUsagesAsync(Guid assetId)
        {
            var asset = await _db.MediaAssets
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == assetId);

            if (asset == null || string.IsNullOrWhiteSpace(asset.FilePath))
            {
                return Array.Empty<CmsMediaAssetUsageDto>();
            }

            var targetNormalized = NormalizePath(asset.FilePath);
            if (string.IsNullOrEmpty(targetNormalized))
            {
                return Array.Empty<CmsMediaAssetUsageDto>();
            }

            var usages = new List<CmsMediaAssetUsageDto>();
            foreach (var descriptor in Descriptors)
            {
                var candidates = await descriptor.FetchCandidatesAsync(_db);
                foreach (var c in candidates)
                {
                    if (PathMatches(c.Path, targetNormalized))
                    {
                        usages.Add(new CmsMediaAssetUsageDto
                        {
                            EntityType = c.EntityType,
                            EntityLabel = c.EntityLabel,
                            EntityId = c.EntityId,
                            PropertyName = c.PropertyName,
                            ControllerName = c.ControllerName,
                            ActionName = c.ActionName,
                            CompanyProfileId = c.CompanyProfileId
                        });
                    }
                }
            }

            return usages;
        }

        public async Task<IReadOnlyDictionary<Guid, int>> GetUsageCountsAsync(Guid? companyProfileId, string? category = null)
        {
            var assets = await GetCandidateAssetsAsync(companyProfileId, category);
            var counts = assets.ToDictionary(a => a.Id, _ => 0);
            var pathToAssetIds = BuildPathIndex(assets);

            if (pathToAssetIds.Count > 0)
            {
                foreach (var descriptor in Descriptors)
                {
                    var candidates = await descriptor.FetchCandidatesAsync(_db);
                    foreach (var c in candidates)
                    {
                        var norm = NormalizePath(c.Path);
                        if (!string.IsNullOrEmpty(norm) && pathToAssetIds.TryGetValue(norm, out var assetIds))
                        {
                            foreach (var id in assetIds) counts[id]++;
                        }
                    }
                }
            }

            return counts;
        }

        private async Task<List<(Guid Id, string FilePath)>> GetCandidateAssetsAsync(Guid? companyProfileId, string? category)
        {
            var query = _db.MediaAssets.AsNoTracking();

            if (companyProfileId.HasValue && companyProfileId.Value != Guid.Empty)
                query = query.Where(a => a.CompanyProfileId == companyProfileId.Value);

            if (!string.IsNullOrWhiteSpace(category) && !category.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                var cat = category.Trim().ToLowerInvariant();
                query = query.Where(a => a.Category != null && a.Category.ToLower() == cat);
            }

            var list = await query.Select(a => new { a.Id, a.FilePath }).ToListAsync();
            return list.Select(x => (x.Id, x.FilePath)).ToList();
        }

        private static Dictionary<string, List<Guid>> BuildPathIndex(List<(Guid Id, string FilePath)> assets)
        {
            var index = new Dictionary<string, List<Guid>>(StringComparer.Ordinal);
            foreach (var (id, path) in assets)
            {
                var norm = NormalizePath(path);
                if (!string.IsNullOrEmpty(norm))
                {
                    if (!index.TryGetValue(norm, out var list))
                    {
                        list = new List<Guid>();
                        index[norm] = list;
                    }
                    list.Add(id);
                }
            }
            return index;
        }

        public async Task<ServiceResult<Guid>> CreateAsync(CmsMediaAssetCreateDto dto, Guid? userId = null)
        {
            if (dto == null)
                return ServiceResult<Guid>.Fail("Media asset data is required.");

            var companyExists = await _db.CompanyProfiles.AnyAsync(c => c.Id == dto.CompanyProfileId);
            if (!companyExists)
                return ServiceResult<Guid>.Fail("Selected company profile does not exist.");

            var entity = new CmsMediaAsset
            {
                Id = Guid.NewGuid(),
                CompanyProfileId = dto.CompanyProfileId,
                FileName = dto.FileName.Trim(),
                OriginalFileName = dto.OriginalFileName.Trim(),
                FilePath = dto.FilePath.Trim(),
                FileSizeBytes = dto.FileSizeBytes,
                ContentType = dto.ContentType?.Trim(),
                AltTextEn = dto.AltTextEn?.Trim(),
                AltTextAr = dto.AltTextAr?.Trim(),
                Category = dto.Category?.Trim() ?? "General",
                Tags = dto.Tags?.Trim(),
                CreatedByUserId = userId
            };

            _db.MediaAssets.Add(entity);
            await _db.SaveChangesAsync();

            return ServiceResult<Guid>.Ok(entity.Id);
        }

        public async Task<ServiceResult> SoftDeleteAsync(Guid id, Guid? userId = null, bool force = false)
        {
            var entity = await _db.MediaAssets.FirstOrDefaultAsync(a => a.Id == id);
            if (entity == null)
                return ServiceResult.Fail("Media asset not found.");

            if (!force)
            {
                var usages = await GetUsagesAsync(id);
                if (usages.Count > 0)
                {
                    var places = usages.Count == 1 ? "1 place" : $"{usages.Count} places";
                    return ServiceResult.Fail($"Cannot delete asset '{entity.OriginalFileName}' because it is currently referenced in {places} across the site.");
                }
            }

            entity.IsDeleted = true;
            entity.CreatedByUserId = userId ?? entity.CreatedByUserId;

            await _db.SaveChangesAsync();
            return ServiceResult.Ok();
        }

        private static string NormalizePath(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;

            return path.Trim().Replace('\\', '/').TrimStart('/').ToLowerInvariant();
        }

        private static bool PathMatches(string? candidatePath, string normalizedTarget)
        {
            if (string.IsNullOrWhiteSpace(candidatePath))
                return false;

            return NormalizePath(candidatePath) == normalizedTarget;
        }

        private sealed class ProjectedUsage
        {
            public Guid Id { get; set; }
            public string? Path { get; set; }
            public string? PrimaryLabel { get; set; }
            public string? SecondaryLabel { get; set; }
            public Guid? CompanyProfileId { get; set; }
        }

        private sealed record UsageCandidate(
            Guid EntityId,
            string? Path,
            string EntityLabel,
            string EntityType,
            string PropertyName,
            string ControllerName,
            string ActionName,
            Guid? CompanyProfileId
        );

        private interface IUsageSourceDescriptor
        {
            Task<List<UsageCandidate>> FetchCandidatesAsync(CmsDbContext db);
        }

        private sealed class UsageSourceDescriptor<TEntity> : IUsageSourceDescriptor where TEntity : class
        {
            private readonly Func<CmsDbContext, DbSet<TEntity>> _dbSet;
            private readonly Expression<Func<TEntity, bool>> _filter;
            private readonly Expression<Func<TEntity, ProjectedUsage>> _projection;
            private readonly string _entityType;
            private readonly string _propertyName;
            private readonly string _controllerName;
            private readonly string _actionName;
            private readonly Func<ProjectedUsage, string> _labelSelector;

            public UsageSourceDescriptor(
                Func<CmsDbContext, DbSet<TEntity>> dbSet,
                Expression<Func<TEntity, bool>> filter,
                Expression<Func<TEntity, ProjectedUsage>> projection,
                string entityType,
                string propertyName,
                string controllerName,
                string actionName,
                Func<ProjectedUsage, string>? labelSelector = null)
            {
                _dbSet = dbSet;
                _filter = filter;
                _projection = projection;
                _entityType = entityType;
                _propertyName = propertyName;
                _controllerName = controllerName;
                _actionName = actionName;
                _labelSelector = labelSelector ?? (r => !string.IsNullOrWhiteSpace(r.PrimaryLabel) ? r.PrimaryLabel! : entityType);
            }

            public async Task<List<UsageCandidate>> FetchCandidatesAsync(CmsDbContext db)
            {
                var rows = await _dbSet(db)
                    .AsNoTracking()
                    .Where(_filter)
                    .Select(_projection)
                    .ToListAsync();

                var list = new List<UsageCandidate>(rows.Count);
                foreach (var r in rows)
                {
                    list.Add(new UsageCandidate(
                        r.Id,
                        r.Path,
                        _labelSelector(r),
                        _entityType,
                        _propertyName,
                        _controllerName,
                        _actionName,
                        r.CompanyProfileId));
                }

                return list;
            }
        }

        private static readonly IUsageSourceDescriptor[] Descriptors = new IUsageSourceDescriptor[]
        {
            new UsageSourceDescriptor<CmsBlogPost>(
                db => db.BlogPosts, b => b.CoverImagePath != null,
                b => new ProjectedUsage { Id = b.Id, Path = b.CoverImagePath, PrimaryLabel = b.TitleEn, CompanyProfileId = b.CompanyProfileId },
                "Blog Post", nameof(CmsBlogPost.CoverImagePath), "CmsBlogPosts", "Edit"),

            new UsageSourceDescriptor<CmsCompanyProfile>(
                db => db.CompanyProfiles, c => c.LogoPath != null,
                c => new ProjectedUsage { Id = c.Id, Path = c.LogoPath, PrimaryLabel = c.NameEn, CompanyProfileId = c.Id },
                "Company Profile", nameof(CmsCompanyProfile.LogoPath), "CmsCompanyProfiles", "Edit"),

            new UsageSourceDescriptor<CmsCompanyProfile>(
                db => db.CompanyProfiles, c => c.FaviconPath != null,
                c => new ProjectedUsage { Id = c.Id, Path = c.FaviconPath, PrimaryLabel = c.NameEn, CompanyProfileId = c.Id },
                "Company Profile", nameof(CmsCompanyProfile.FaviconPath), "CmsCompanyProfiles", "Edit"),

            new UsageSourceDescriptor<CmsDivisionPage>(
                db => db.DivisionPages, d => d.CoverImagePath != null,
                d => new ProjectedUsage { Id = d.Id, Path = d.CoverImagePath, PrimaryLabel = d.HeroTitleEn, SecondaryLabel = d.Slug, CompanyProfileId = d.CompanyProfileId },
                "Division Page", nameof(CmsDivisionPage.CoverImagePath), "CmsDivisionPages", "Edit",
                r => !string.IsNullOrWhiteSpace(r.PrimaryLabel) ? r.PrimaryLabel : (!string.IsNullOrWhiteSpace(r.SecondaryLabel) ? $"Division ({r.SecondaryLabel})" : "Division Page")),

            new UsageSourceDescriptor<CmsProject>(
                db => db.Projects, p => p.CoverImagePath != null,
                p => new ProjectedUsage { Id = p.Id, Path = p.CoverImagePath, PrimaryLabel = p.TitleEn, CompanyProfileId = p.CompanyProfileId },
                "Project", nameof(CmsProject.CoverImagePath), "CmsProjects", "Edit"),

            new UsageSourceDescriptor<CmsService>(
                db => db.Services, s => s.ImagePath != null,
                s => new ProjectedUsage { Id = s.Id, Path = s.ImagePath, PrimaryLabel = s.NameEn, CompanyProfileId = s.CompanyProfileId },
                "Service", nameof(CmsService.ImagePath), "CmsServices", "Edit"),

            new UsageSourceDescriptor<CmsTeamMember>(
                db => db.TeamMembers, t => t.PhotoPath != null,
                t => new ProjectedUsage { Id = t.Id, Path = t.PhotoPath, PrimaryLabel = t.NameEn, CompanyProfileId = t.CompanyProfileId },
                "Team Member", nameof(CmsTeamMember.PhotoPath), "CmsTeamMembers", "Edit"),

            new UsageSourceDescriptor<CmsTestimonial>(
                db => db.Testimonials, t => t.AuthorPhotoPath != null,
                t => new ProjectedUsage { Id = t.Id, Path = t.AuthorPhotoPath, PrimaryLabel = t.AuthorNameEn, SecondaryLabel = t.CompanyName, CompanyProfileId = t.CompanyProfileId },
                "Testimonial", nameof(CmsTestimonial.AuthorPhotoPath), "CmsTestimonials", "Edit",
                r => !string.IsNullOrWhiteSpace(r.PrimaryLabel) ? (!string.IsNullOrWhiteSpace(r.SecondaryLabel) ? $"{r.PrimaryLabel} ({r.SecondaryLabel})" : r.PrimaryLabel) : "Testimonial"),

            new UsageSourceDescriptor<CmsHumanVisionSection>(
                db => db.HumanVisionSections, h => h.ImagePath != null,
                h => new ProjectedUsage { Id = h.Id, Path = h.ImagePath, PrimaryLabel = h.BadgeEn, CompanyProfileId = h.CompanyProfileId },
                "Human Vision Section", nameof(CmsHumanVisionSection.ImagePath), "CmsHomeSections", "EditHumanVision"),

            new UsageSourceDescriptor<CmsPioneersSection>(
                db => db.PioneersSections, p => p.ImagePath != null,
                p => new ProjectedUsage { Id = p.Id, Path = p.ImagePath, PrimaryLabel = p.BadgeEn, CompanyProfileId = p.CompanyProfileId },
                "Pioneers Section", nameof(CmsPioneersSection.ImagePath), "CmsHomeSections", "EditPioneers",
                r => !string.IsNullOrWhiteSpace(r.PrimaryLabel) ? r.PrimaryLabel : "Message to Pioneers Section"),

            new UsageSourceDescriptor<CmsEthicsSection>(
                db => db.EthicsSections, e => e.TopImagePath != null,
                e => new ProjectedUsage { Id = e.Id, Path = e.TopImagePath, PrimaryLabel = e.BadgeEn, CompanyProfileId = e.CompanyProfileId },
                "Ethics Section", nameof(CmsEthicsSection.TopImagePath), "CmsHomeSections", "EditEthics",
                r => !string.IsNullOrWhiteSpace(r.PrimaryLabel) ? r.PrimaryLabel : "Operational Strategy & Ethics Section"),

            new UsageSourceDescriptor<CmsEthicsSection>(
                db => db.EthicsSections, e => e.BottomImagePath != null,
                e => new ProjectedUsage { Id = e.Id, Path = e.BottomImagePath, PrimaryLabel = e.BadgeEn, CompanyProfileId = e.CompanyProfileId },
                "Ethics Section", nameof(CmsEthicsSection.BottomImagePath), "CmsHomeSections", "EditEthics",
                r => !string.IsNullOrWhiteSpace(r.PrimaryLabel) ? r.PrimaryLabel : "Operational Strategy & Ethics Section"),

            new UsageSourceDescriptor<CmsEthicsSection>(
                db => db.EthicsSections, e => e.BackgroundImagePath != null,
                e => new ProjectedUsage { Id = e.Id, Path = e.BackgroundImagePath, PrimaryLabel = e.BadgeEn, CompanyProfileId = e.CompanyProfileId },
                "Ethics Section", nameof(CmsEthicsSection.BackgroundImagePath), "CmsHomeSections", "EditEthics",
                r => !string.IsNullOrWhiteSpace(r.PrimaryLabel) ? r.PrimaryLabel : "Operational Strategy & Ethics Section"),

            new UsageSourceDescriptor<CmsCtaSection>(
                db => db.CtaSections, c => c.BackgroundImagePath != null,
                c => new ProjectedUsage { Id = c.Id, Path = c.BackgroundImagePath, PrimaryLabel = c.BadgeEn, CompanyProfileId = c.CompanyProfileId },
                "CTA Section", nameof(CmsCtaSection.BackgroundImagePath), "CmsHomeSections", "EditCta",
                r => !string.IsNullOrWhiteSpace(r.PrimaryLabel) ? r.PrimaryLabel : "Call to Action Section"),
        };
    }
}
