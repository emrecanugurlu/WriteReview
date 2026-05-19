using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WriteReview.Domain.Entities.EnumClass;
using WriteReview.Persistence.Contexts;

namespace WriteReview.Persistence.Services.Review
{
    public class ReviewService
    {
        private readonly WriteReviewDbContext _db;

        public ReviewService(WriteReviewDbContext db)
        {
            _db = db;
        }

        public async Task<object?> GetReviewByArticleIdAsync(Guid id)
        {
            var exists = await _db.Articles.AsNoTracking().AnyAsync(x => x.Id == id);
            if (!exists)
                throw new Exception("Article not found.");

            return await _db.ArticleReviews
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
