using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WriteReview.Domain.Entities;

namespace WriteReview.Persistence.Contexts
{
    public class WriteReviewDbContext : IdentityDbContext<AppUser,AppRole,Guid> 
    {
        public DbSet<Article> Articles { get; set; }
        public DbSet<ArticleVersion> ArticleVersions { get; set; }
        public DbSet<ArticleReview> ArticleReviews { get; set; }
        public DbSet<ArticleExpertAssignment> ArticleExpertAssignments { get; set; }
        public DbSet<ExpertiseArea> ExpertiseAreas { get; set; }
        public DbSet<UserExpertiseArea> UserExpertiseAreas { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<AppNotification> Notifications { get; set; }
        public WriteReviewDbContext(DbContextOptions options) : base(options)
        {
        }

        protected WriteReviewDbContext()
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Article>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.AuthorId).IsRequired();
                entity.Property(a => a.Status).HasConversion<int>();


                entity.HasOne(a => a.Author)
                .WithMany(u => u.Articles)
                .HasForeignKey(a => a.AuthorId);
            });

            builder.Entity<ArticleVersion>(b =>
            {
                b.HasKey(x => x.Id);
                
                b.HasOne(x => x.Article)
                 .WithMany(a => a.Versions)
                 .HasForeignKey(x => x.ArticleId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<ArticleReview>(b =>
            {
                b.HasKey(x => x.Id);

                b.HasOne(x => x.Article)
                 .WithMany(a => a.Reviews)            
                 .HasForeignKey(x => x.ArticleId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(x => x.ArticleVersion)
                 .WithMany()
                 .HasForeignKey(x => x.ArticleVersionId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.Reviewer)
                 .WithMany()                         
                 .HasForeignKey(x => x.ReviewerId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.Property(x => x.Action).HasConversion<int>();
                b.Property(x => x.FromStatus).HasConversion<int>();
                b.Property(x => x.ToStatus).HasConversion<int>();
                b.Property(x => x.Note).HasMaxLength(2000);
                b.Property(x => x.Reason).HasMaxLength(2000);
            });


            builder.Entity<ArticleExpertAssignment>(b =>
            {
                b.HasKey(x => new {x.ArticleId,x.ExpertId});

                b.HasOne(x => x.Article)
                 .WithMany(a => a.ExpertAssignments)
                 .HasForeignKey(x => x.ArticleId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(x => x.Expert)
                 .WithMany()
                 .HasForeignKey(x => x.ExpertId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<UserExpertiseArea>(b =>
            {
                b.ToTable("UserExpertiseArea");
                b.HasKey(ue => new {ue.UserId,ue.ExpertiseAreaId});

                b.HasOne(ue => ue.User)
                .WithMany(e => e.ExpertiseAreas)
                .HasForeignKey(ue => ue.UserId);

                b.HasOne(ue => ue.ExpertiseArea)
                .WithMany(e => e.Users)
                .HasForeignKey(ue=> ue.ExpertiseAreaId);

            });

            builder.Entity<Category>(b =>
            {
                b.HasKey(c => c.Id);
                b.Property(c => c.Name).IsRequired().HasMaxLength(100);

                b.HasMany(c => c.Articles)
                 .WithOne(a => a.Category)
                 .HasForeignKey(a => a.CategoryId);
            });

            builder.Entity<AppNotification>(b =>
            {
                b.HasKey(n => n.Id);
                b.HasOne(n => n.User)
                 .WithMany()
                 .HasForeignKey(n => n.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });




            base.OnModelCreating(builder);
        }
    }
}
