using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WriteReview.Domain.Dtos
{
    public sealed class TakeToReviewRequest
    {
        public string? Note { get; set; }
        public List<Guid> ExpertIds { get; set; } = new();
    }
}
