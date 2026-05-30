using System;

namespace WriteReview.Domain.Entities
{
    public class AppNotification
    {
        public Guid Id { get; set; }
        
        // Alıcı Kullanıcı
        public Guid UserId { get; set; }
        public AppUser User { get; set; }

        public string Title { get; set; }
        public string Message { get; set; }
        
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }

        public AppNotification()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            IsRead = false;
        }
    }
}
