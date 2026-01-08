using System.ComponentModel.DataAnnotations;

namespace SonnaBackend.Models
{
    public class Project
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? FileUrl { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Accepted, Rejected
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation property
        public User? User { get; set; }
    }
}
