using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WriteReview.Domain.Entities
{
    public class UserExpertiseArea
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public Guid UserId { get; set; }
        public AppUser User { get; set; } = new AppUser();

        public Guid ExpertiseAreaId { get; set; }
        public ExpertiseArea ExpertiseArea { get; set; } = new ExpertiseArea();
    }
}
