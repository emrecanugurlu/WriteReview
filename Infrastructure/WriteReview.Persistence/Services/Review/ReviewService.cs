using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WriteReview.Persistence.Contexts;

namespace WriteReview.Persistence.Services.Review
{
    internal class ReviewService
    {

        public void GetReviewByArticleId(Guid id, WriteReviewDbContext writeReviewDbContext)
        {
            var article = writeReviewDbContext.Articles
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (article == null)
            {
                throw new Exception("Article not found.");
            }

            writeReviewDbContext.ArticleReviews
                .AsNoTracking()
                .Where(r => r.ArticleId == id)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    type = "staff",
                    action = r.Action,
                    note = r.Note,
                    reason = r.Reason,
                    fromStatus = r.FromStatus,
                    toStatus = r.ToStatus,
                    createdAt = r.CreatedAt,
                    reviewerId = r.ReviewerId
                })
                .ToListAsync();

        }
    }
}
