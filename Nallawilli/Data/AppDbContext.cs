using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Nallawilli.Models.Entities;

namespace Nallawilli.Data
{
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // =====================
        // DBSets
        // =====================
        public DbSet<Post> Posts { get; set; }
        public DbSet<Category> Categories { get; set; }

        // CMS
        public DbSet<CmsPage> CmsPages { get; set; }
        public DbSet<CmsSection> CmsSections { get; set; }
        public DbSet<CmsSectionContent> CmsSectionContents { get; set; }

        // =====================
        // CONFIG
        // =====================
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =====================
            // POST
            // =====================
            modelBuilder.Entity<Post>()
                .HasIndex(x => x.Slug)
                .IsUnique();

            modelBuilder.Entity<Post>()
                .Property(x => x.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Post>()
                .HasQueryFilter(x => x.DeletedAt == null);

            // =====================
            // CATEGORY
            // =====================
            modelBuilder.Entity<Category>()
                .HasIndex(x => x.Slug)
                .IsUnique();

            modelBuilder.Entity<Category>()
                .HasQueryFilter(x => x.DeletedAt == null);

            // =====================
            // CMS PAGE
            // =====================
            modelBuilder.Entity<CmsPage>()
                .HasIndex(x => x.Slug)
                .IsUnique();

            modelBuilder.Entity<CmsPage>()
                .HasQueryFilter(x => x.DeletedAt == null);

            modelBuilder.Entity<CmsPage>()
                .HasMany(p => p.Sections)
                .WithOne(s => s.Page)
                .HasForeignKey(s => s.PageId)
                .OnDelete(DeleteBehavior.Cascade);

            // =====================
            // CMS SECTION
            // =====================
            modelBuilder.Entity<CmsSection>()
                .HasIndex(x => x.PageId);

            modelBuilder.Entity<CmsSection>()
                .HasQueryFilter(x => x.DeletedAt == null);

            modelBuilder.Entity<CmsSection>()
                .HasMany(s => s.SectionContents)
                .WithOne(c => c.Section)
                .HasForeignKey(c => c.SectionId)
                .OnDelete(DeleteBehavior.Cascade);

            // =====================
            // CMS SECTION CONTENT
            // =====================
            modelBuilder.Entity<CmsSectionContent>()
                .Property(x => x.ContentValue)
                .HasColumnType("nvarchar(max)");

            modelBuilder.Entity<CmsSectionContent>()
                .HasIndex(x => new { x.SectionId, x.ContentKey })
                .IsUnique();

            modelBuilder.Entity<CmsSectionContent>()
                .HasQueryFilter(x => x.DeletedAt == null);
        }
    }
}