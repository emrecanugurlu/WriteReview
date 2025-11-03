using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WriteReview.Domain.Dtos
{
    public class ArticleReviewDto
    {

        public Guid Id { get; set; }
        public int Action { get; set; }                 
        public string? Note { get; set; }
        public string? Reason { get; set; }
        public int FromStatus { get; set; }             
        public int ToStatus { get; set; }               
        public Guid ReviewerId { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
