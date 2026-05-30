using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WriteReview.Domain.Entities.EnumClass
{
    public enum ArticleStatus
    {
        Draft = 0,
        Submitted = 1,
        InReview = 2,
        Approved = 3,
        Rejected = 4,
        RevisionsRequested = 5,
        /// <summary>
        /// Yazar reddedilen makaleye itiraz etti, Manager karar bekliyor.
        /// </summary>
        AppealPending = 6
    }
}
