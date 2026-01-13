using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WriteReview.Domain.Dtos
{
    public sealed class ExpertAssignmentDto
    {
        public Guid ArticleId { get; set; }
        public string ArticleTitle { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string ArticleCategory { get; set; } = string.Empty;
        public int Status { get; set; }             
        public DateTime? ReviewedAt { get; set; }
    }
}
