using Microsoft.EntityFrameworkCore;
using Neurix.DAL.Models;

namespace Neurix.DAL.Data
{
    /// <summary>
    /// Database context for the public Content Management System (CMS).
    /// Kept strictly isolated from CrmDbContext.
    /// </summary>
    public class CmsDbContext : DbContext
    {
        private readonly CmsContentRevision? _contentRevision;

        public CmsDbContext(DbContextOptions<CmsDbContext> options, CmsContentRevision? contentRevision = null)
            : base(options)
        {
            _contentRevision = contentRevision;
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            var saved = base.SaveChanges(acceptAllChangesOnSuccess);
            if (saved > 0) _contentRevision?.Advance();
            return saved;
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            var saved = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            if (saved > 0) _contentRevision?.Advance();
            return saved;
        }

        public DbSet<CmsCompanyProfile> CompanyProfiles => Set<CmsCompanyProfile>();
        public DbSet<CmsService> Services => Set<CmsService>();
        public DbSet<CmsSocialLink> SocialLinks => Set<CmsSocialLink>();
        public DbSet<CmsSiteSetting> SiteSettings => Set<CmsSiteSetting>();
        public DbSet<CmsTeamMember> TeamMembers => Set<CmsTeamMember>();
        public DbSet<CmsBlogPost> BlogPosts => Set<CmsBlogPost>();
        public DbSet<CmsTestimonial> Testimonials => Set<CmsTestimonial>();
        public DbSet<CmsProject> Projects => Set<CmsProject>();
        public DbSet<CmsDivisionPage> DivisionPages => Set<CmsDivisionPage>();
        public DbSet<CmsMediaAsset> MediaAssets => Set<CmsMediaAsset>();
        public DbSet<CmsHeroSection> HeroSections => Set<CmsHeroSection>();
        public DbSet<CmsHumanVisionSection> HumanVisionSections => Set<CmsHumanVisionSection>();
        public DbSet<CmsPioneersSection> PioneersSections => Set<CmsPioneersSection>();
        public DbSet<CmsPillarsSection> PillarsSections => Set<CmsPillarsSection>();
        public DbSet<CmsPillarItem> PillarItems => Set<CmsPillarItem>();
        public DbSet<CmsDivisionsSection> DivisionsSections => Set<CmsDivisionsSection>();
        public DbSet<CmsDivisionItem> DivisionItems => Set<CmsDivisionItem>();
        public DbSet<CmsAiEngineeringSection> AiEngineeringSections => Set<CmsAiEngineeringSection>();
        public DbSet<CmsAiEngineeringItem> AiEngineeringItems => Set<CmsAiEngineeringItem>();
        public DbSet<CmsListSectionHeader> ListSectionHeaders => Set<CmsListSectionHeader>();
        public DbSet<CmsPageBand> PageBands => Set<CmsPageBand>();
        public DbSet<CmsPageBandItem> PageBandItems => Set<CmsPageBandItem>();
        public DbSet<CmsEthicsSection> EthicsSections => Set<CmsEthicsSection>();
        public DbSet<CmsCtaSection> CtaSections => Set<CmsCtaSection>();
        public DbSet<CmsMenuItem> MenuItems => Set<CmsMenuItem>();
        public DbSet<CmsContentPage> ContentPages => Set<CmsContentPage>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<CmsCompanyProfile>(entity =>
            {
                entity.ToTable("CmsCompanyProfiles");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Slug).IsRequired().HasMaxLength(100);
                entity.Property(e => e.NameEn).IsRequired().HasMaxLength(200);
                entity.Property(e => e.NameAr).IsRequired().HasMaxLength(200);
                entity.Property(e => e.LogoPath).HasMaxLength(500);
                entity.Property(e => e.FaviconPath).HasMaxLength(500);
                entity.Property(e => e.PrimaryColor).HasMaxLength(50);
                entity.Property(e => e.AccentColor).HasMaxLength(50);
                entity.Property(e => e.TaglineEn).HasMaxLength(300);
                entity.Property(e => e.TaglineAr).HasMaxLength(300);
                entity.Property(e => e.ShortDescriptionEn).HasMaxLength(500);
                entity.Property(e => e.ShortDescriptionAr).HasMaxLength(500);
                entity.Property(e => e.DescriptionEn).HasMaxLength(4000);
                entity.Property(e => e.DescriptionAr).HasMaxLength(4000);
                entity.Property(e => e.Email).HasMaxLength(256);
                entity.Property(e => e.Phone).HasMaxLength(50);
                entity.Property(e => e.AddressEn).HasMaxLength(500);
                entity.Property(e => e.AddressAr).HasMaxLength(500);
                entity.Property(e => e.Website).HasMaxLength(300);

                entity.HasIndex(e => e.Slug).IsUnique();
                entity.HasIndex(e => e.IsPublished);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            builder.Entity<CmsService>(entity =>
            {
                entity.ToTable("CmsServices");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Slug).IsRequired().HasMaxLength(100);
                entity.Property(e => e.NameEn).IsRequired().HasMaxLength(200);
                entity.Property(e => e.NameAr).IsRequired().HasMaxLength(200);
                entity.Property(e => e.ShortDescriptionEn).HasMaxLength(500);
                entity.Property(e => e.ShortDescriptionAr).HasMaxLength(500);
                entity.Property(e => e.DescriptionEn).HasMaxLength(4000);
                entity.Property(e => e.DescriptionAr).HasMaxLength(4000);
                entity.Property(e => e.IconName).HasMaxLength(100);
                entity.Property(e => e.ImagePath).HasMaxLength(500);

                entity.HasIndex(e => new { e.CompanyProfileId, e.Slug }).IsUnique();
                entity.HasIndex(e => e.CompanyProfileId);
                entity.HasIndex(e => e.IsPublished);
                entity.HasIndex(e => e.DisplayOrder);

                entity.HasOne(e => e.CompanyProfile)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            builder.Entity<CmsSocialLink>(entity =>
            {
                entity.ToTable("CmsSocialLinks");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Platform).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Url).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.DisplayName).HasMaxLength(100);
                entity.Property(e => e.IconName).HasMaxLength(100);

                entity.HasIndex(e => e.CompanyProfileId);
                entity.HasIndex(e => e.IsPublished);
                entity.HasIndex(e => e.DisplayOrder);

                entity.HasOne(e => e.CompanyProfile)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            builder.Entity<CmsSiteSetting>(entity =>
            {
                entity.ToTable("CmsSiteSettings");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Key).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ValueEn).HasMaxLength(4000);
                entity.Property(e => e.ValueAr).HasMaxLength(4000);
                entity.Property(e => e.SettingType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.GroupName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Label).IsRequired().HasMaxLength(200);

                entity.HasIndex(e => new { e.CompanyProfileId, e.Key }).IsUnique();
                entity.HasIndex(e => e.CompanyProfileId);
                entity.HasIndex(e => e.GroupName);

                entity.HasOne(e => e.CompanyProfile)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            builder.Entity<CmsTeamMember>(entity =>
            {
                entity.ToTable("CmsTeamMembers");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.NameEn).IsRequired().HasMaxLength(200);
                entity.Property(e => e.NameAr).IsRequired().HasMaxLength(200);
                entity.Property(e => e.TitleEn).IsRequired().HasMaxLength(200);
                entity.Property(e => e.TitleAr).IsRequired().HasMaxLength(200);
                entity.Property(e => e.BioEn).HasMaxLength(1000);
                entity.Property(e => e.BioAr).HasMaxLength(1000);
                entity.Property(e => e.PhotoPath).HasMaxLength(500);
                entity.Property(e => e.LinkedInUrl).HasMaxLength(500);
                entity.Property(e => e.Email).HasMaxLength(256);

                entity.HasIndex(e => e.CompanyProfileId);
                entity.HasIndex(e => e.IsPublished);
                entity.HasIndex(e => e.DisplayOrder);

                entity.HasOne(e => e.CompanyProfile)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            builder.Entity<CmsBlogPost>(entity =>
            {
                entity.ToTable("CmsBlogPosts");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Slug).IsRequired().HasMaxLength(150);
                entity.Property(e => e.TitleEn).IsRequired().HasMaxLength(300);
                entity.Property(e => e.TitleAr).IsRequired().HasMaxLength(300);
                entity.Property(e => e.SummaryEn).HasMaxLength(1000);
                entity.Property(e => e.SummaryAr).HasMaxLength(1000);
                entity.Property(e => e.CoverImagePath).HasMaxLength(500);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.AuthorNameEn).HasMaxLength(200);
                entity.Property(e => e.AuthorNameAr).HasMaxLength(200);
                entity.Property(e => e.Tags).HasMaxLength(500);

                entity.HasIndex(e => new { e.CompanyProfileId, e.Slug }).IsUnique();
                entity.HasIndex(e => e.CompanyProfileId);
                entity.HasIndex(e => e.IsPublished);
                entity.HasIndex(e => e.IsFeatured);
                entity.HasIndex(e => e.PublishedAtUtc);

                entity.HasOne(e => e.CompanyProfile)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            builder.Entity<CmsTestimonial>(entity =>
            {
                entity.ToTable("CmsTestimonials");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.AuthorNameEn).IsRequired().HasMaxLength(200);
                entity.Property(e => e.AuthorNameAr).IsRequired().HasMaxLength(200);
                entity.Property(e => e.AuthorTitleEn).HasMaxLength(200);
                entity.Property(e => e.AuthorTitleAr).HasMaxLength(200);
                entity.Property(e => e.CompanyName).HasMaxLength(200);
                entity.Property(e => e.AuthorPhotoPath).HasMaxLength(500);
                entity.Property(e => e.QuoteEn).IsRequired().HasMaxLength(1500);
                entity.Property(e => e.QuoteAr).IsRequired().HasMaxLength(1500);

                entity.HasIndex(e => e.CompanyProfileId);
                entity.HasIndex(e => e.IsPublished);
                entity.HasIndex(e => e.IsFeatured);
                entity.HasIndex(e => e.DisplayOrder);

                entity.HasOne(e => e.CompanyProfile)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            builder.Entity<CmsProject>(entity =>
            {
                entity.ToTable("CmsProjects");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Slug).IsRequired().HasMaxLength(150);
                entity.Property(e => e.TitleEn).IsRequired().HasMaxLength(300);
                entity.Property(e => e.TitleAr).IsRequired().HasMaxLength(300);
                entity.Property(e => e.SummaryEn).HasMaxLength(1000);
                entity.Property(e => e.SummaryAr).HasMaxLength(1000);
                entity.Property(e => e.CoverImagePath).HasMaxLength(500);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.ClientName).HasMaxLength(200);
                entity.Property(e => e.TechnologiesUsed).HasMaxLength(500);
                entity.Property(e => e.ProjectUrl).HasMaxLength(500);
                entity.Property(e => e.GithubUrl).HasMaxLength(500);

                entity.HasIndex(e => new { e.CompanyProfileId, e.Slug }).IsUnique();
                entity.HasIndex(e => e.CompanyProfileId);
                entity.HasIndex(e => e.IsPublished);
                entity.HasIndex(e => e.IsFeatured);
                entity.HasIndex(e => e.DisplayOrder);

                entity.HasOne(e => e.CompanyProfile)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            builder.Entity<CmsDivisionPage>(entity =>
            {
                entity.ToTable("CmsDivisionPages");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Slug).IsRequired().HasMaxLength(100);
                entity.Property(e => e.HeroTitleEn).IsRequired().HasMaxLength(300);
                entity.Property(e => e.HeroTitleAr).IsRequired().HasMaxLength(300);
                entity.Property(e => e.HeroSubtitleEn).HasMaxLength(1000);
                entity.Property(e => e.HeroSubtitleAr).HasMaxLength(1000);
                entity.Property(e => e.MissionEn).HasMaxLength(2000);
                entity.Property(e => e.MissionAr).HasMaxLength(2000);
                entity.Property(e => e.CoverImagePath).HasMaxLength(500);

                entity.HasIndex(e => new { e.CompanyProfileId, e.Slug }).IsUnique();
                entity.HasIndex(e => e.CompanyProfileId);
                entity.HasIndex(e => e.IsPublished);

                entity.HasOne(e => e.CompanyProfile)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            builder.Entity<CmsMediaAsset>(entity =>
            {
                entity.ToTable("CmsMediaAssets");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.OriginalFileName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
                entity.Property(e => e.ContentType).HasMaxLength(100);
                entity.Property(e => e.AltTextEn).HasMaxLength(300);
                entity.Property(e => e.AltTextAr).HasMaxLength(300);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.Tags).HasMaxLength(500);

                entity.HasIndex(e => e.CompanyProfileId);
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.CreatedAtUtc);

                entity.HasOne(e => e.CompanyProfile)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            builder.Entity<CmsHeroSection>(entity =>
            {
                entity.ToTable("CmsHeroSections");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.BadgeEn).HasMaxLength(200);
                entity.Property(e => e.BadgeAr).HasMaxLength(200);
                entity.Property(e => e.TitlePrefixEn).HasMaxLength(200);
                entity.Property(e => e.TitleHighlightEn).HasMaxLength(200);
                entity.Property(e => e.TitleSuffixEn).HasMaxLength(200);
                entity.Property(e => e.TitlePrefixAr).HasMaxLength(200);
                entity.Property(e => e.TitleHighlightAr).HasMaxLength(200);
                entity.Property(e => e.TitleSuffixAr).HasMaxLength(200);
                entity.Property(e => e.SubtitleEn).HasMaxLength(1000);
                entity.Property(e => e.SubtitleAr).HasMaxLength(1000);
                entity.Property(e => e.PrimaryButtonTextEn).HasMaxLength(100);
                entity.Property(e => e.PrimaryButtonTextAr).HasMaxLength(100);
                entity.Property(e => e.PrimaryButtonUrl).HasMaxLength(500);
                entity.Property(e => e.SecondaryButtonTextEn).HasMaxLength(100);
                entity.Property(e => e.SecondaryButtonTextAr).HasMaxLength(100);
                entity.Property(e => e.SecondaryButtonUrl).HasMaxLength(500);
                entity.Property(e => e.Stat1Value).HasMaxLength(50);
                entity.Property(e => e.Stat1LabelEn).HasMaxLength(100);
                entity.Property(e => e.Stat1LabelAr).HasMaxLength(100);
                entity.Property(e => e.Stat2Value).HasMaxLength(50);
                entity.Property(e => e.Stat2LabelEn).HasMaxLength(100);
                entity.Property(e => e.Stat2LabelAr).HasMaxLength(100);
                entity.Property(e => e.Stat3Value).HasMaxLength(50);
                entity.Property(e => e.Stat3LabelEn).HasMaxLength(100);
                entity.Property(e => e.Stat3LabelAr).HasMaxLength(100);

                entity.HasIndex(e => e.CompanyProfileId).IsUnique();
                entity.HasIndex(e => e.IsPublished);

                entity.HasOne(e => e.CompanyProfile)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            builder.Entity<CmsHumanVisionSection>(entity =>
            {
                entity.ToTable("CmsHumanVisionSections");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.BadgeEn).HasMaxLength(200);
                entity.Property(e => e.BadgeAr).HasMaxLength(200);
                entity.Property(e => e.TitlePrefixEn).HasMaxLength(200);
                entity.Property(e => e.TitleHighlightEn).HasMaxLength(200);
                entity.Property(e => e.TitlePrefixAr).HasMaxLength(200);
                entity.Property(e => e.TitleHighlightAr).HasMaxLength(200);
                entity.Property(e => e.Paragraph1En).HasMaxLength(4000);
                entity.Property(e => e.Paragraph1Ar).HasMaxLength(4000);
                entity.Property(e => e.Paragraph2En).HasMaxLength(4000);
                entity.Property(e => e.Paragraph2Ar).HasMaxLength(4000);
                entity.Property(e => e.ImagePath).HasMaxLength(500);
                entity.Property(e => e.ImageAltEn).HasMaxLength(200);
                entity.Property(e => e.ImageAltAr).HasMaxLength(200);

                entity.HasIndex(e => e.CompanyProfileId).IsUnique();
                entity.HasIndex(e => e.IsPublished);

                entity.HasOne(e => e.CompanyProfile)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            builder.Entity<CmsPioneersSection>(entity =>
            {
                entity.ToTable("CmsPioneersSections");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.BadgeEn).HasMaxLength(200);
                entity.Property(e => e.BadgeAr).HasMaxLength(200);
                entity.Property(e => e.TitlePrefixEn).HasMaxLength(200);
                entity.Property(e => e.TitleHighlightEn).HasMaxLength(200);
                entity.Property(e => e.TitlePrefixAr).HasMaxLength(200);
                entity.Property(e => e.TitleHighlightAr).HasMaxLength(200);
                entity.Property(e => e.Paragraph1En).HasMaxLength(4000);
                entity.Property(e => e.Paragraph1Ar).HasMaxLength(4000);
                entity.Property(e => e.Paragraph2En).HasMaxLength(4000);
                entity.Property(e => e.Paragraph2Ar).HasMaxLength(4000);
                entity.Property(e => e.ImagePath).HasMaxLength(500);
                entity.Property(e => e.ImageAltEn).HasMaxLength(200);
                entity.Property(e => e.ImageAltAr).HasMaxLength(200);

                entity.HasIndex(e => e.CompanyProfileId).IsUnique();
                entity.HasIndex(e => e.IsPublished);

                entity.HasOne(e => e.CompanyProfile)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            builder.Entity<CmsPillarsSection>(entity =>
            {
                entity.ToTable("CmsPillarsSections");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.TitleEn).HasMaxLength(200);
                entity.Property(e => e.TitleAr).HasMaxLength(200);
                entity.Property(e => e.SubtitleEn).HasMaxLength(2000);
                entity.Property(e => e.SubtitleAr).HasMaxLength(2000);

                entity.HasIndex(e => e.CompanyProfileId).IsUnique();
                entity.HasIndex(e => e.IsPublished);

                entity.HasOne(e => e.CompanyProfile)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            builder.Entity<CmsPillarItem>(entity =>
            {
                entity.ToTable("CmsPillarItems");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.IconName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.TitleEn).IsRequired().HasMaxLength(200);
                entity.Property(e => e.TitleAr).IsRequired().HasMaxLength(200);
                entity.Property(e => e.DescriptionEn).HasMaxLength(2000);
                entity.Property(e => e.DescriptionAr).HasMaxLength(2000);

                entity.HasIndex(e => new { e.CompanyProfileId, e.DisplayOrder });
                entity.HasIndex(e => e.IsPublished);

                entity.HasOne(e => e.CompanyProfile)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            builder.Entity<CmsDivisionsSection>(entity =>
            {
                entity.ToTable("CmsDivisionsSections");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.BadgeEn).HasMaxLength(200);
                entity.Property(e => e.BadgeAr).HasMaxLength(200);
                entity.Property(e => e.TitlePrefixEn).HasMaxLength(200);
                entity.Property(e => e.TitleHighlightEn).HasMaxLength(200);
                entity.Property(e => e.TitlePrefixAr).HasMaxLength(200);
                entity.Property(e => e.TitleHighlightAr).HasMaxLength(200);
                entity.Property(e => e.DescriptionEn).HasMaxLength(2000);
                entity.Property(e => e.DescriptionAr).HasMaxLength(2000);

                entity.HasIndex(e => e.CompanyProfileId).IsUnique();
                entity.HasIndex(e => e.IsPublished);

                entity.HasOne(e => e.CompanyProfile)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            builder.Entity<CmsDivisionItem>(entity =>
            {
                entity.ToTable("CmsDivisionItems");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.IconName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.TitleEn).IsRequired().HasMaxLength(200);
                entity.Property(e => e.TitleAr).IsRequired().HasMaxLength(200);
                entity.Property(e => e.DescriptionEn).HasMaxLength(2000);
                entity.Property(e => e.DescriptionAr).HasMaxLength(2000);
                entity.Property(e => e.LinkUrl).IsRequired().HasMaxLength(500);
                entity.Property(e => e.LinkTextEn).HasMaxLength(100);
                entity.Property(e => e.LinkTextAr).HasMaxLength(100);

                entity.HasIndex(e => new { e.CompanyProfileId, e.DisplayOrder });
                entity.HasIndex(e => e.IsPublished);

                entity.HasOne(e => e.CompanyProfile)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            builder.Entity<CmsAiEngineeringSection>(entity =>
            {
                entity.ToTable("CmsAiEngineeringSections");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.BadgeEn).HasMaxLength(200);
                entity.Property(e => e.BadgeAr).HasMaxLength(200);
                entity.Property(e => e.TitleEn).HasMaxLength(200);
                entity.Property(e => e.TitleAr).HasMaxLength(200);

                entity.HasIndex(e => e.CompanyProfileId).IsUnique();
                entity.HasIndex(e => e.IsPublished);

                entity.HasOne(e => e.CompanyProfile)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            builder.Entity<CmsAiEngineeringItem>(entity =>
            {
                entity.ToTable("CmsAiEngineeringItems");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.IconName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.TitleEn).IsRequired().HasMaxLength(200);
                entity.Property(e => e.TitleAr).IsRequired().HasMaxLength(200);
                entity.Property(e => e.DescriptionEn).HasMaxLength(2000);
                entity.Property(e => e.DescriptionAr).HasMaxLength(2000);

                entity.HasIndex(e => new { e.CompanyProfileId, e.DisplayOrder });
                entity.HasIndex(e => e.IsPublished);

                entity.HasOne(e => e.CompanyProfile)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            builder.Entity<CmsListSectionHeader>(entity =>
            {
                entity.ToTable("CmsListSectionHeaders");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.SectionKey).IsRequired().HasMaxLength(50);
                entity.Property(e => e.BadgeEn).HasMaxLength(200);
                entity.Property(e => e.BadgeAr).HasMaxLength(200);
                entity.Property(e => e.TitlePrefixEn).HasMaxLength(200);
                entity.Property(e => e.TitleHighlightEn).HasMaxLength(200);
                entity.Property(e => e.TitlePrefixAr).HasMaxLength(200);
                entity.Property(e => e.TitleHighlightAr).HasMaxLength(200);
                entity.Property(e => e.SubtitleEn).HasMaxLength(2000);
                entity.Property(e => e.SubtitleAr).HasMaxLength(2000);
                entity.Property(e => e.ItemLinkTextEn).HasMaxLength(100);
                entity.Property(e => e.ItemLinkTextAr).HasMaxLength(100);
                entity.Property(e => e.DefaultCategoryLabelEn).HasMaxLength(100);
                entity.Property(e => e.DefaultCategoryLabelAr).HasMaxLength(100);
                entity.Property(e => e.ReadTimeSuffixEn).HasMaxLength(50);
                entity.Property(e => e.ReadTimeSuffixAr).HasMaxLength(50);
                entity.Property(e => e.UndatedLabelEn).HasMaxLength(100);
                entity.Property(e => e.UndatedLabelAr).HasMaxLength(100);
                entity.Property(e => e.ButtonTextEn).HasMaxLength(100);
                entity.Property(e => e.ButtonTextAr).HasMaxLength(100);
                entity.Property(e => e.ButtonUrl).HasMaxLength(500);

                entity.HasIndex(e => new { e.CompanyProfileId, e.SectionKey }).IsUnique();
                entity.HasIndex(e => e.IsPublished);

                entity.HasOne(e => e.CompanyProfile)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            builder.Entity<CmsPageBand>(entity =>
            {
                entity.ToTable("CmsPageBands");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.PageKey).IsRequired().HasMaxLength(50);
                entity.Property(e => e.BandKey).IsRequired().HasMaxLength(50);
                entity.Property(e => e.BadgeEn).HasMaxLength(200);
                entity.Property(e => e.BadgeAr).HasMaxLength(200);
                entity.Property(e => e.TitlePrefixEn).HasMaxLength(300);
                entity.Property(e => e.TitlePrefixAr).HasMaxLength(300);
                entity.Property(e => e.TitleHighlightEn).HasMaxLength(300);
                entity.Property(e => e.TitleHighlightAr).HasMaxLength(300);
                entity.Property(e => e.TitleSuffixEn).HasMaxLength(300);
                entity.Property(e => e.TitleSuffixAr).HasMaxLength(300);
                entity.Property(e => e.BodyEn).HasMaxLength(4000);
                entity.Property(e => e.BodyAr).HasMaxLength(4000);
                entity.Property(e => e.ImagePath).HasMaxLength(500);
                entity.Property(e => e.ButtonTextEn).HasMaxLength(100);
                entity.Property(e => e.ButtonTextAr).HasMaxLength(100);
                entity.Property(e => e.ButtonUrl).HasMaxLength(500);
                entity.Property(e => e.ItemLinkTextEn).HasMaxLength(100);
                entity.Property(e => e.ItemLinkTextAr).HasMaxLength(100);
                entity.Property(e => e.EmptyStateEn).HasMaxLength(1000);
                entity.Property(e => e.EmptyStateAr).HasMaxLength(1000);

                entity.HasIndex(e => new { e.CompanyProfileId, e.PageKey, e.BandKey }).IsUnique();
                entity.HasIndex(e => e.IsPublished);

                entity.HasOne(e => e.CompanyProfile)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            builder.Entity<CmsPageBandItem>(entity =>
            {
                entity.ToTable("CmsPageBandItems");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.PageKey).IsRequired().HasMaxLength(50);
                entity.Property(e => e.BandKey).IsRequired().HasMaxLength(50);
                entity.Property(e => e.IconName).HasMaxLength(100);
                entity.Property(e => e.TitleEn).IsRequired().HasMaxLength(300);
                entity.Property(e => e.TitleAr).IsRequired().HasMaxLength(300);
                entity.Property(e => e.DescriptionEn).HasMaxLength(2000);
                entity.Property(e => e.DescriptionAr).HasMaxLength(2000);
                entity.Property(e => e.ImagePath).HasMaxLength(500);
                entity.Property(e => e.LinkUrl).HasMaxLength(500);
                entity.Property(e => e.LinkTextEn).HasMaxLength(100);
                entity.Property(e => e.LinkTextAr).HasMaxLength(100);

                entity.HasIndex(e => new { e.CompanyProfileId, e.PageKey, e.BandKey, e.DisplayOrder });
                entity.HasIndex(e => e.IsPublished);

                entity.HasOne(e => e.CompanyProfile)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            builder.Entity<CmsEthicsSection>(entity =>
            {
                entity.ToTable("CmsEthicsSections");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.BadgeEn).HasMaxLength(200);
                entity.Property(e => e.BadgeAr).HasMaxLength(200);
                entity.Property(e => e.TitlePrefixEn).HasMaxLength(200);
                entity.Property(e => e.TitleHighlightEn).HasMaxLength(200);
                entity.Property(e => e.TitlePrefixAr).HasMaxLength(200);
                entity.Property(e => e.TitleHighlightAr).HasMaxLength(200);
                entity.Property(e => e.DescriptionEn).HasMaxLength(4000);
                entity.Property(e => e.DescriptionAr).HasMaxLength(4000);
                entity.Property(e => e.TopImagePath).HasMaxLength(500);
                entity.Property(e => e.TopImageAltEn).HasMaxLength(200);
                entity.Property(e => e.TopImageAltAr).HasMaxLength(200);
                entity.Property(e => e.BottomImagePath).HasMaxLength(500);
                entity.Property(e => e.BottomImageAltEn).HasMaxLength(200);
                entity.Property(e => e.BottomImageAltAr).HasMaxLength(200);
                entity.Property(e => e.BackgroundImagePath).HasMaxLength(500);

                entity.HasIndex(e => e.CompanyProfileId).IsUnique();
                entity.HasIndex(e => e.IsPublished);

                entity.HasOne(e => e.CompanyProfile)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            builder.Entity<CmsCtaSection>(entity =>
            {
                entity.ToTable("CmsCtaSections");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.BadgeEn).HasMaxLength(100);
                entity.Property(e => e.BadgeAr).HasMaxLength(100);
                entity.Property(e => e.TitlePrefixEn).HasMaxLength(200);
                entity.Property(e => e.TitleHighlightEn).HasMaxLength(200);
                entity.Property(e => e.TitlePrefixAr).HasMaxLength(200);
                entity.Property(e => e.TitleHighlightAr).HasMaxLength(200);
                entity.Property(e => e.ButtonTextEn).HasMaxLength(100);
                entity.Property(e => e.ButtonTextAr).HasMaxLength(100);
                entity.Property(e => e.ButtonUrl).HasMaxLength(500);
                entity.Property(e => e.ContactEmail).HasMaxLength(200);
                entity.Property(e => e.BackgroundImagePath).HasMaxLength(500);

                entity.HasIndex(e => e.CompanyProfileId).IsUnique();
                entity.HasIndex(e => e.IsPublished);

                entity.HasOne(e => e.CompanyProfile)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            builder.Entity<CmsMenuItem>(entity =>
            {
                entity.ToTable("CmsMenuItems");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.LabelEn).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LabelAr).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Url).IsRequired().HasMaxLength(500);
                entity.Property(e => e.IconName).HasMaxLength(100);
                entity.Property(e => e.Placement).HasConversion<int>().IsRequired();
                entity.Property(e => e.DisplayOrder).HasDefaultValue(0);
                entity.Property(e => e.OpenInNewTab).HasDefaultValue(false);
                entity.Property(e => e.IsPublished).HasDefaultValue(true);
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);

                entity.HasIndex(e => e.CompanyProfileId);
                entity.HasIndex(e => new { e.CompanyProfileId, e.Placement, e.IsPublished });
                entity.HasIndex(e => e.DisplayOrder);

                entity.HasOne(e => e.CompanyProfile)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            builder.Entity<CmsContentPage>(entity =>
            {
                entity.ToTable("CmsContentPages");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Slug).IsRequired().HasMaxLength(100);
                entity.Property(e => e.TitleEn).IsRequired().HasMaxLength(300);
                entity.Property(e => e.TitleAr).IsRequired().HasMaxLength(300);
                entity.Property(e => e.MetaDescriptionEn).HasMaxLength(500);
                entity.Property(e => e.MetaDescriptionAr).HasMaxLength(500);

                entity.Property(e => e.HeroBadgeEn).HasMaxLength(150);
                entity.Property(e => e.HeroBadgeAr).HasMaxLength(150);
                entity.Property(e => e.HeroTitlePrefixEn).HasMaxLength(300);
                entity.Property(e => e.HeroTitlePrefixAr).HasMaxLength(300);
                entity.Property(e => e.HeroTitleHighlightEn).HasMaxLength(300);
                entity.Property(e => e.HeroTitleHighlightAr).HasMaxLength(300);
                entity.Property(e => e.HeroSubtitleEn).HasMaxLength(2000);
                entity.Property(e => e.HeroSubtitleAr).HasMaxLength(2000);

                entity.Property(e => e.MissionTitleEn).HasMaxLength(200);
                entity.Property(e => e.MissionTitleAr).HasMaxLength(200);
                entity.Property(e => e.MissionTextEn).HasMaxLength(4000);
                entity.Property(e => e.MissionTextAr).HasMaxLength(4000);
                entity.Property(e => e.VisionTitleEn).HasMaxLength(200);
                entity.Property(e => e.VisionTitleAr).HasMaxLength(200);
                entity.Property(e => e.VisionTextEn).HasMaxLength(4000);
                entity.Property(e => e.VisionTextAr).HasMaxLength(4000);

                entity.Property(e => e.CtaBadgeEn).HasMaxLength(150);
                entity.Property(e => e.CtaBadgeAr).HasMaxLength(150);
                entity.Property(e => e.CtaTitleEn).HasMaxLength(300);
                entity.Property(e => e.CtaTitleAr).HasMaxLength(300);
                entity.Property(e => e.CtaSubtitleEn).HasMaxLength(1000);
                entity.Property(e => e.CtaSubtitleAr).HasMaxLength(1000);
                entity.Property(e => e.CtaButtonTextEn).HasMaxLength(150);
                entity.Property(e => e.CtaButtonTextAr).HasMaxLength(150);

                // BodyEn/BodyAr and ExtraDataJson stay unbounded (nvarchar(max)) — a privacy
                // or terms document can be long.

                entity.Property(e => e.IsPublished).HasDefaultValue(true);
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);

                // One page per slug per brand — this is what GetBySlugAsync looks up.
                entity.HasIndex(e => new { e.CompanyProfileId, e.Slug }).IsUnique();
                entity.HasIndex(e => e.CompanyProfileId);
                entity.HasIndex(e => e.IsPublished);

                entity.HasOne(e => e.CompanyProfile)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            StampAuditFields();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            StampAuditFields();
            return base.SaveChanges();
        }

        private void StampAuditFields()
        {
            var now = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<IAuditable>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAtUtc = now;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAtUtc = now;
                    entry.Property(nameof(IAuditable.CreatedAtUtc)).IsModified = false;
                }
            }
        }
    }
}
