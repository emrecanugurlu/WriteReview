using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WriteReview.Domain.Entities.EnumClass
{
    public enum ReviewAction
    {
        TakeToReview = 1,     // Submitted → InReview
        Approve = 2,          // InReview  → Approved
        Reject = 3,           // InReview  → Rejected
        RequestRevision = 4,  // InReview  → RevisionsRequested
        AppealSubmitted = 5,  // Rejected  → AppealPending  (Yazar itiraz etti)
        AppealAccepted = 6,   // AppealPending → InReview   (Manager kabul etti)
        AppealDenied = 7      // AppealPending → Rejected   (Manager reddetti)
    }
}
