namespace WriteReview.Domain.Dtos
{
    public sealed class TakeToReviewRequest
    {
        public string? Note { get; set; }
        public List<Guid> ExpertIds { get; set; } = new();
    }
}
