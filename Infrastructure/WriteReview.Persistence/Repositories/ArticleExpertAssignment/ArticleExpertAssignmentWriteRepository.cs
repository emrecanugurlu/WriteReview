using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WriteReview.Application.Repositories.ArticleExpertAssignment;
using WriteReview.Domain.Dtos.RequestDto;
using WriteReview.Persistence.Contexts;
using WriteReview.Domain.Entities;
using WriteReview.Domain.Dtos;

namespace WriteReview.Persistence.Repositories.ArticleExpertAssignment
{
    public class ArticleExpertAssignmentWriteRepository : WriteRepository<Domain.Entities.ArticleExpertAssignment>, IArticleExpertAssignmentWriteRepository
    {
        public ArticleExpertAssignmentWriteRepository(WriteReviewDbContext context) : base(context)
        {
        }

        public async Task<Result<string>> AddArticleExpertsAssignment(AddArticleExpertsRequestDto dto)
        {
            try
            {
                var articleId = Guid.Parse(dto.articleId);
                var successCount = 0;
                var skipCount = 0;
                var errors = new List<string>();

                foreach (var expertId in dto.expertsId)
                {
                    var expertGuid = Guid.Parse(expertId);

                    var exist = await Table.AsNoTracking()
                        .AnyAsync(x => x.ArticleId == articleId && x.ExpertId == expertGuid);

                    if (exist)
                    {
                        skipCount++;
                        continue; 
                    }

                    try
                    {
                        var assignment = new Domain.Entities.ArticleExpertAssignment
                        {
                            ArticleId = articleId,
                            ExpertId = expertGuid,
                        };

                        await Table.AddAsync(assignment);
                        await SaveChangesAsync();
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Expert {expertId}: {ex.Message}");
                    }
                }

                var message = $"{successCount} uzman başarıyla atandı";
                if (skipCount > 0)
                    message += $", {skipCount} uzman zaten atanmış";
                if (errors.Any())
                    message += $", {errors.Count} hata oluştu";

                return Result<string>.Success(message);
            }
            catch (Exception ex)
            {
                return Result<string>.Failure($"İşlem başarısız: {ex.Message}");
            }
        }

    }
}
