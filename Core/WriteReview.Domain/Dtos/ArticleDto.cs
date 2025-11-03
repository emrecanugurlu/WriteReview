using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WriteReview.Domain.Dtos
{
    /// <summary>
    /// Bu sınıf, bir makalenin temel bilgilerini taşımak için kullanılır. Makale kimliği, başlığı, durumu ve son güncellenme tarihini içerir.
    /// </summary>
    public class ArticleDto
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = "";
        public int Status { get; init; }
        public DateTime UpdatedAt { get; init; }
    }
}
