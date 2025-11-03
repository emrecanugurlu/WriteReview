using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WriteReview.Domain.Dtos.ResponseDto
{
    public class ExpertiseAreaWithoutUsers
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
    }
}
