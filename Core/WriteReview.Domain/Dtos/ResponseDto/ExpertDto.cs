using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WriteReview.Domain.Dtos.ResponseDto
{
    /***
     * Bu sınıf, uzmanlarla ilgili yanıt verilerini temsil eder.
     ***/
    public class ExpertDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<string> ExpertiseAreas { get; set; }
        public int ActiveTasks { get; set; }
    }
}
