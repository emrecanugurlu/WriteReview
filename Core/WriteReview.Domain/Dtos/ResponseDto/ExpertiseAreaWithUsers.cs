using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WriteReview.Domain.Entities;

namespace WriteReview.Domain.Dtos.ResponseDto
{
    public class ExpertiseAreaWithUsers
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public List<ExpertiseAreaUser> Users { get; set; }
    }
}
