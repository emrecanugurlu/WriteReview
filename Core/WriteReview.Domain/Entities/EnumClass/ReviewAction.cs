using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WriteReview.Domain.Entities.EnumClass
{
    public enum ReviewAction
    {
        TakeToReview = 1,  // Submitted -> InReview
        Approve = 2,       // InReview  -> Approved
        Reject = 3,        // InReview  -> Rejected
        RequestRevision = 4// InReview  -> RevisionsRequested
    }
}
