using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WriteReview.Domain.Dtos;
using WriteReview.Domain.Entities;

namespace WriteReview.Application.Services
{
    public interface IArticleStateService
    {
        /// <summary>
        /// Taslak olan veriyi gönderilmiş duruma getirir.
        /// </summary>
        /// <param name="article"></param>
        public void DraftToSubmitted(Article article);
        /// <summary>
        ///  Gönderilmiş olan veriyi incelemede duruma getirir.
        /// </summary>
        /// <param name="article"></param>
        /// <param name="note"></param>
        public void SubmittedToInReview(Article article, string? note);
        /// <summary>
        /// İncelemede olan veriyi onaylanmış duruma getirir.
        /// </summary>
        /// <param name="article"></param>
        /// <param name="note"></param>
        public void InReviewToApproved(Article article, string? note);
        /// <summary>
        /// İncelemede olan veriyi reddedilmiş duruma getirir.
        /// </summary>
        /// <param name="article"></param>
        /// <param name="rejectionDto"></param>
        public void InReviewToRejected(Article article, RejectionDto rejectionDto);
        /// <summary>
        /// İncelemede olan veriyi revizyon istenmiş duruma getirir.
        /// </summary>
        /// <param name="article"></param>
        /// <param name="note"></param>
        public void InReviewToRevisionsRequested(Article article, string note);
        /// <summary>
        /// Revizyon istenen makaleyi yazar düzelttikten sonra tekrar gönderir.
        /// </summary>
        /// <param name="article"></param>
        public void RevisionsRequestedToSubmitted(Article article);
    }
}
