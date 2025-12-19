using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WriteReview.Domain.Dtos.Staff
{
    public sealed class ManagerArticleListItemDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = "";
        public Guid AuthorId { get; set; }
        public string AuthorName { get; set; } = default!;
        public List<string> Experts { get; set; } = default!;
        public int Status { get; set; }
        public string Category { get; set; } = default!;
        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
