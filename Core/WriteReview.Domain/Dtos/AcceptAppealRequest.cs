namespace WriteReview.Domain.Dtos
{
    /// <summary>
    /// Manager'ın itirazı kabul ederken gönderebileceği veri.
    /// </summary>
    public sealed class AcceptAppealRequest
    {
        /// <summary>Karar notu (opsiyonel).</summary>
        public string? Note { get; set; }

        /// <summary>
        /// Yeniden veya ek atanacak hakem ID'leri (opsiyonel).
        /// Daha önce atanmış olanlar tekrar eklenmez.
        /// </summary>
        public List<Guid> ExpertIds { get; set; } = new();
    }
}
