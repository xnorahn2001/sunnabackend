using System.ComponentModel.DataAnnotations;

namespace SonnaBackend.Models
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        public string FullName { get; set; } = string.Empty;
        
        [Required]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        
        [Required]
        public string UserType { get; set; } = "Individual"; // Admin, Individual, Factory
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        public string? CRNumber { get; set; } // Commercial Registration Number
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation property
        public List<Project> Projects { get; set; } = new();
    }
}
