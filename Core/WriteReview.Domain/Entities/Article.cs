using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WriteReview.Domain.Entities.EnumClass;

namespace WriteReview.Domain.Entities
{
    public class Article
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string ContentPath { get; set; } = string.Empty;
        public Guid AuthorId { get; set; }
        public AppUser Author { get; set; }
        public ICollection<ArticleReview> Reviews { get; set; } = new List<ArticleReview>();
        public ICollection<ArticleExpertAssignment> ExpertAssignments { get; set; } = new List<ArticleExpertAssignment>();
        public ArticleStatus Status { get; set; } = ArticleStatus.Draft;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
