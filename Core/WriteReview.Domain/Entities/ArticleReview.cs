using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WriteReview.Domain.Entities.EnumClass;

namespace WriteReview.Domain.Entities
{
    public class ArticleReview
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ArticleId { get; set; }
        public Article Article { get; set; } = default!;
        /// <summary>
        /// Review yapan personel kimliği
        /// </summary>
        public Guid ReviewerId { get; set; }  
        public AppUser Reviewer { get; set; } = default!;

        public ReviewAction Action { get; set; }
        /// <summary>
        /// Makale onaylanırsa veya düzeltilirse eklenen not
        /// </summary>
        public string? Note { get; set; }   
        /// <summary>
        /// Eğer makale reddedilirse, reddetme nedeni
        /// </summary>
        public string? Reason { get; set; } 

        public ArticleStatus FromStatus { get; set; }
        public ArticleStatus ToStatus { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}



