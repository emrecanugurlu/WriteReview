using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WriteReview.Domain.Dtos;
using WriteReview.Domain.Dtos.RequestDto;

namespace WriteReview.Application.Repositories.ArticleExpertAssignment
{
    public interface IArticleExpertAssignmentWriteRepository: IWriteRepository<Domain.Entities.ArticleExpertAssignment>
    {
        Task<Result<string>> AddArticleExpertsAssignment(AddArticleExpertsRequestDto dto);
    }
}
