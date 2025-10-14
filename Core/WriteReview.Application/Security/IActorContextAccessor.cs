using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WriteReview.Domain.Security;

namespace WriteReview.Application.Security
{
    public interface IActorContextAccessor
    {
        ActorContext GetCurrent();
    }
}
