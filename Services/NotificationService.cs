using MimeKit;
using MailKit.Net.Smtp;

namespace SonnaBackend.Services
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string subject, string message);
    }

    public class NotificationService : INotificationService
    {
        private readonly IConfiguration _configuration;

        public NotificationService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendNotificationAsync(string subject, string messageBody)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");
            var host = emailSettings["Host"];
            var port = int.Parse(emailSettings["Port"] ?? "587");
            var username = emailSettings["Username"];
            var password = emailSettings["Password"];
            var sonnaEmail = emailSettings["SonnaEmail"]; // The recipient

            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(sonnaEmail))
            {
                // Log or ignore if config is missing to avoid crashing in dev
                Console.WriteLine($"[Mock Email] To: {sonnaEmail}, Subject: {subject}, Body: {messageBody}");
                return;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Sonna System", username));
            message.To.Add(new MailboxAddress("Sonna Admin", sonnaEmail));
            message.Subject = subject;

            message.Body = new TextPart("plain")
            {
                Text = messageBody
            };

            using (var client = new SmtpClient())
            {
                try 
                {
                    await client.ConnectAsync(host, port, false);
                    await client.AuthenticateAsync(username, password);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending email: {ex.Message}");
                }
            }
        }
    }
}
