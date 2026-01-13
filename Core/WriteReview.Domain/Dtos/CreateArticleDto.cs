using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WriteReview.Domain.Dtos
{
    public class CreateArticleDto
    {
        public string Title { get; set; } = default!;
        public  string Summary { get; set; } = default!;
        public  string Content { get; set; } = default!;
        public string CategoryId { get; set; } = default!;
        public string Tags { get; set; } = default!;
    }
}
