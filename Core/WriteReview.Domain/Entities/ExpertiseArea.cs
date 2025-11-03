using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WriteReview.Domain.Entities
{
    /// <summary>
    /// Bu entity sınıfı kullanıcı rolu "expert" olanların uzmanlık alanlarını belkirlemek için kullanılır.
    /// </summary>
    public class ExpertiseArea
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public ICollection<UserExpertiseArea> Users { get; set; } = new List<UserExpertiseArea>();
    }
}
