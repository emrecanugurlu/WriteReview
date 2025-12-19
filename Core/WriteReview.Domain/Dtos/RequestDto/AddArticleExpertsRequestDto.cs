using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WriteReview.Domain.Dtos.RequestDto
{
    public sealed class AddArticleExpertsRequestDto
    {
        public string articleId { get; set; }
        public string[] expertsId { get; set; }
    }
}
