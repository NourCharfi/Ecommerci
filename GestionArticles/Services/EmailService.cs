using System.Net.Mail;

namespace GestionArticles.Services
{
    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string htmlBody);
    }

    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _config;
        public EmailService(ILogger<EmailService> logger, IConfiguration config)
        {
            _logger = logger; _config = config;
        }
        public async Task SendAsync(string to, string subject, string htmlBody)
        {
            // Simplifié: log-only. Brancher SMTP réel via config (SmtpClient) si besoin.
            _logger.LogInformation($"Email to {to}: {subject}");
            await Task.CompletedTask;
        }
    }
}
