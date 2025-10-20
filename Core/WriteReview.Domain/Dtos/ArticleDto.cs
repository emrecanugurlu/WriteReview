using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WriteReview.Domain.Dtos
{
    internal class ArticleDto
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = "";
        public int Status { get; init; }
        public DateTime UpdatedAt { get; init; }
    }
}
