namespace WriteReview.Domain.Dtos
{
    public sealed class AppealRequest
    {
        /// <summary>
        /// İtiraz gerekçesi – zorunlu, en az 20 karakter.
        /// </summary>
        public string Reason { get; set; } = string.Empty;
    }
}
