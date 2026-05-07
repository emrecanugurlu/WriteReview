using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WriteReview.Domain.Entities;

namespace WriteReview.Domain.Dtos
{
    /// <summary>
    /// Bu sınıf, bir makalenin temel bilgilerini taşımak için kullanılır. Makale kimliği, başlığı, durumu ve son güncellenme tarihini içerir.
    /// </summary>
    public class ArticleDto
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = "";
        public string Summary { get; init; } = default!;
        public string Content { get; set; } = default!;
        public string AuthorName { get; set; }
        public string Category { get; set; }
        public int Status { get; init; }
        public DateTime UpdatedAt { get; init; }
        public List<ArticleExpertSummaryDto> Experts { get; set; } = new();
    }

    public class ArticleExpertSummaryDto
    {
        public string ExpertName { get; set; } = string.Empty;
        public int Status { get; set; }
        public string? Feedback { get; set; }
    }
}
