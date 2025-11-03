using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WriteReview.Domain.Entities.EnumClass;

namespace WriteReview.Domain.Entities
{
    public class ArticleExpertAssignment
    {
        public Guid ArticleId { get; set; }
        public Article Article { get; set; } = default!;
        public Guid ExpertId { get; set; }
        public AppUser Expert { get; set; } = default!;
        public string? Feedback { get; set; }
        public int? Score { get; set; }
        public DateTime ReviewedAt { get; set; } = DateTime.UtcNow;
        public ExpertAssignmentStatus Status{ get; set; } = ExpertAssignmentStatus.Pending;
    }
}
