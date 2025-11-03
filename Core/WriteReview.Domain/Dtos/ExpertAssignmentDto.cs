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
        public string ArticleTitle { get; set; } = "";
        public int ArticleStatus { get; set; }
        public int Status { get; set; }             
        public string? Feedback { get; set; }
        public int? Score { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }
}
