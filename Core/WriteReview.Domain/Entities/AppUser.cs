
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WriteReview.Domain.Entities
{
    public class AppUser : IdentityUser<Guid>
    {
        public string FullName { get; set; } = default!;
        public ICollection<Article>? Articles { get; set; }
        public ICollection<UserExpertiseArea> ExpertiseAreas { get; set; } = new List<UserExpertiseArea>();
    }
}












