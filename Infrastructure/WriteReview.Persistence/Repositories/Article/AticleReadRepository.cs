using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WriteReview.Application.Repositories.Article;
using WriteReview.Domain.Dtos;
using WriteReview.Domain.Entities.EnumClass;
using WriteReview.Persistence.Contexts;

namespace WriteReview.Persistence.Repositories.Article
{
    public class ArticleReadRepository : ReadRepository<Domain.Entities.Article>, IArticleReadRepository
    {
        public ArticleReadRepository(WriteReviewDbContext context) : base(context)
        {
        }

        async public Task<ArticleDto> GetArticleWithCategoryAndAuthor(string articleId)
        {
            var article = await Table
                .Include(a => a.Category)
                .Include(a => a.Author)
                .Include(a => a.ExpertAssignments)
                    .ThenInclude(ea => ea.Expert)
                .FirstOrDefaultAsync(a => a.Id.ToString() == articleId);

            // Makale AppealPending durumundaysa itiraz gerekçesini çek
            string? appealReason = null;
            if (article.Status == ArticleStatus.AppealPending)
            {
                var appealReview = await _context.ArticleReviews
                    .Where(r => r.ArticleId == article.Id && r.Action == ReviewAction.AppealSubmitted)
                    .OrderByDescending(r => r.CreatedAt)
                    .FirstOrDefaultAsync();
                appealReason = appealReview?.Note;
            }

            var articleDto = new Domain.Dtos.ArticleDto
            {
                Status = (int)article.Status,
                Title = article.Title,
                Content = article.ContentPath,
                CategoryId = article.CategoryId,
                Category = article.Category != null ? article.Category.Name : null,
                AuthorName = article.Author.FullName,
                Id = article.Id,
                UpdatedAt = article.UpdatedAt,
                AppealReason = appealReason,
                Experts = article.ExpertAssignments?.Select(ea => new ArticleExpertSummaryDto
                {
                    ExpertId = ea.ExpertId,
                    ExpertName = ea.Expert?.FullName ?? "",
                    Status = (int)ea.Status,
                    Feedback = ea.Feedback
                }).ToList() ?? new List<ArticleExpertSummaryDto>()
            };

            return articleDto;
        }
    }
}
