using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WriteReview.Domain.Dtos.Staff
{
    public sealed class StaffArticleListItemDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = "";
        public Guid AuthorId { get; set; }
        public int Status { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
