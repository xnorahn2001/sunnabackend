namespace SonnaBackend.Controllers.DTOs
{
    public class LoginDto
    {
        public string PhoneOrCR { get; set; } = string.Empty; // Changed from Email
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string UserType { get; set; } = "Individual";
        public string? CRNumber { get; set; }
    }
}

