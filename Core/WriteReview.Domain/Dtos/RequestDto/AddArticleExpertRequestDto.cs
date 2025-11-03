using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WriteReview.Domain.Dtos.RequestDto
{
    public sealed class AddArticleExpertRequestDto
    {
        public Guid ArticleId { get; set; }
        public Guid ExpertId { get; set; }
    }
}
