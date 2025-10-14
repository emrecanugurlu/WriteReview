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
            base.OnModelCreating(builder);
        }
    }
}
