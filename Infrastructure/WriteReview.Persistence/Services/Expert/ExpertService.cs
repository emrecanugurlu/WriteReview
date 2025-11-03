using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WriteReview.Domain.Dtos.RequestDto;
using WriteReview.Domain.Entities;
using WriteReview.Persistence.Contexts;

namespace WriteReview.Persistence.Services.Expert
{
    public class ExpertService
    {
        public async Task AddAssignmentAsync(WriteReviewDbContext db, AddArticleExpertRequestDto dto)
        {
            
            await db.ArticleExpertAssignments.AddAsync(new ArticleExpertAssignment
            {
                ArticleId = dto.ArticleId,
                ExpertId = dto.ExpertId,

            });

            await db.SaveChangesAsync();

        }
    }
}
