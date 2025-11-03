using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WriteReview.Domain.Dtos;

namespace WriteReview.Domain.Dtos.RequestDto
{
    public class CreateArticleRequest
    {
        public required CreateArticleDto ArticleDto { get; set; }
        public bool IsSubmit { get; set; } = false;
    }
}
