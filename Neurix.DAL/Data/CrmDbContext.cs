using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Neurix.DAL.Models;

namespace Neurix.DAL.Data
{
    /// <summary>
    /// Database context for the private CRM (NeurixCRM database).
    /// </summary>
    public class CrmDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public CrmDbContext(DbContextOptions<CrmDbContext> options)
            : base(options)
        {
        }

        public DbSet<Company> Companies => Set<Company>();
        public DbSet<Contact> Contacts => Set<Contact>();
        public DbSet<Lead> Leads => Set<Lead>();
        public DbSet<Deal> Deals => Set<Deal>();
        public DbSet<NewsletterSubscription> NewsletterSubscriptions => Set<NewsletterSubscription>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Company>(entity =>
            {
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Industry).HasMaxLength(100);
                entity.Property(e => e.Website).HasMaxLength(300);
                entity.Property(e => e.Phone).HasMaxLength(50);
                entity.Property(e => e.Email).HasMaxLength(256);
                entity.Property(e => e.Address).HasMaxLength(300);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.Country).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(2000);

                entity.HasIndex(e => e.Name);
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            builder.Entity<Contact>(entity =>
            {
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.JobTitle).HasMaxLength(150);
                entity.Property(e => e.Email).HasMaxLength(256);
                entity.Property(e => e.Phone).HasMaxLength(50);
                entity.Property(e => e.Notes).HasMaxLength(2000);

                entity.HasIndex(e => e.Email);

                // Restrict guards against accidental hard deletes. The business rule
                // (a company with active contacts cannot be removed) is enforced in
                // CompanyService, because soft delete is an UPDATE and never trips an FK.
                entity.HasOne(e => e.Company)
                      .WithMany(c => c.Contacts)
                      .HasForeignKey(e => e.CompanyId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            builder.Entity<Lead>(entity =>
            {
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
                entity.Property(e => e.CompanyName).HasMaxLength(200);
                entity.Property(e => e.Phone).HasMaxLength(50);
                entity.Property(e => e.InquiryType).HasMaxLength(100);
                entity.Property(e => e.Message).HasMaxLength(2000);
                entity.Property(e => e.Notes).HasMaxLength(2000);

                entity.HasIndex(e => e.Email);
                entity.HasIndex(e => e.Status);

                // Deleting a staff account leaves their leads in place, unassigned.
                entity.HasOne(e => e.AssignedToUser)
                      .WithMany()
                      .HasForeignKey(e => e.AssignedToUserId)
                      .OnDelete(DeleteBehavior.SetNull);

                // Restrict, like Contact -> Company: a lead's record of what it was
                // converted into should not disappear behind its own back.
                entity.HasOne(e => e.ConvertedToCompany)
                      .WithMany()
                      .HasForeignKey(e => e.ConvertedToCompanyId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ConvertedToContact)
                      .WithMany()
                      .HasForeignKey(e => e.ConvertedToContactId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            builder.Entity<Deal>(entity =>
            {
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Notes).HasMaxLength(2000);

                // Money needs a fixed scale; the SQL Server default for decimal
                // would silently truncate to whole numbers.
                entity.Property(e => e.Value).HasPrecision(18, 2);

                entity.HasIndex(e => e.Stage);

                // Restrict, like Contact -> Company. The business rule (a company
                // with open deals cannot be removed) lives in CompanyService,
                // because soft delete is an UPDATE and never trips an FK.
                entity.HasOne(e => e.Company)
                      .WithMany()
                      .HasForeignKey(e => e.CompanyId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Losing the named contact should not lose the deal itself.
                entity.HasOne(e => e.Contact)
                      .WithMany()
                      .HasForeignKey(e => e.ContactId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.AssignedToUser)
                      .WithMany()
                      .HasForeignKey(e => e.AssignedToUserId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            builder.Entity<NewsletterSubscription>(entity =>
            {
                entity.Property(e => e.Email).IsRequired().HasMaxLength(256);

                // One row per address across active and soft-deleted rows, so a
                // resubscribe reactivates instead of producing duplicates.
                entity.HasIndex(e => e.Email).IsUnique();

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
