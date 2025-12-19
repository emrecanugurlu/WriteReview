using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WriteReview.Domain.Dtos.ResponseDto
{
    public class AssignmentArticleDetailDto
    {
        public Guid ArticleId { get; set; }
        public string ArticleTitle { get; set; } = string.Empty;
        public string ArticleSummary { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string ArticleCategory { get; set; } = string.Empty;
        public string ArticleContent { get; set; } = string.Empty;
        public int Status { get; set; }
        public string? Feedback { get; set; }
        public int? Score { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }
}
