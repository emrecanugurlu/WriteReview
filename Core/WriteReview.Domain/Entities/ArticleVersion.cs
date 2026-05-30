using System;

namespace WriteReview.Domain.Entities
{
    public class ArticleVersion
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ArticleId { get; set; }
        public Article Article { get; set; } = default!;
        
        /// <summary>
        /// Versiyon numarası (Örn: 1, 2, 3)
        /// </summary>
        public int VersionNumber { get; set; }
        
        /// <summary>
        /// O versiyona ait HTML içeriğinin dosya yolu
        /// </summary>
        public string ContentPath { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
