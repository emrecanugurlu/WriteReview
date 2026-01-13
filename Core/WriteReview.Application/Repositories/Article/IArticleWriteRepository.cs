using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WriteReview.Application.Security;
using WriteReview.Application.Services;
using WriteReview.Domain.Dtos;
using WriteReview.Domain.Entities;


namespace WriteReview.Application.Repositories.Article
{
    public interface IArticleWriteRepository : IWriteRepository<Domain.Entities.Article>    
    {
        public Task<string> AddArticleWithAuthor(IActorContextAccessor actor, CreateArticleDto createArticleDto, bool isSubmit, IArticleStateService articleStateService);
    }
}
