using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WriteReview.Domain.Dtos
{
    public sealed class ArticleListItemDto
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public int Status { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string Category { get; set; }
    }
}
