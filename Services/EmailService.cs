namespace PFMS_MI04.Services
{
    using MailKit.Net.Smtp;
    using MimeKit;
    using Microsoft.Extensions.Configuration;
    using System.Threading.Tasks;

    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> SendEmailAsync(string recipientEmail, string subject, string message, string recipientName)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress(
                _configuration["SmtpSettings:SenderName"],
                _configuration["SmtpSettings:SenderEmail"]));
            emailMessage.To.Add(new MailboxAddress("", recipientEmail));
            emailMessage.Subject = subject;

            emailMessage.Body = new TextPart("html")
            {
                Text = $"Dear {recipientName},<br><br>" +
                       $"{message}<br><br>" +
                       "Yours Sincerely,<br>" +
                       "Miri Red Crescent Dialysis Center"
            };

            using (var client = new SmtpClient())
            {
                try
                {
                    await client.ConnectAsync(
                        _configuration["SmtpSettings:Server"],
                        int.Parse(_configuration["SmtpSettings:Port"]),
                        bool.Parse(_configuration["SmtpSettings:UseSSL"]));

                    await client.AuthenticateAsync(
                        _configuration["SmtpSettings:Username"],
                        _configuration["SmtpSettings:Password"]);

                    await client.SendAsync(emailMessage);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending email: {ex.Message}");
                    return false;
                }
                finally
                {
                    await client.DisconnectAsync(true);
                }
            }
        }

    }

}
