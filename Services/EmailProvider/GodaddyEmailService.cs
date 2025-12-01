using System.Net.Mail;
using System.Net;
using TraceWebApi.Interfaces;

namespace TraceWebApi.Services.EmailProvider;

public class GodaddyEmailService : IEmailService
{
    private readonly string smtpServer = "smtp.office365.com";  // For Office 365 or Microsoft 365 email
    private readonly int smtpPort = 587;  // Use port 587 for STARTTLS
    private readonly string senderEmail = "traceaidevelopment@gmail.com";  // Replace with your GoDaddy email address
    private readonly string senderPassword = "f9Gm2wRzQ1";  // Replace with your email password

    public Task SendEmailAsync(string to, string subject, string body)
    {
        SmtpClient smtpClient = new SmtpClient(smtpServer)
        {
            Port = smtpPort,
            Credentials = new NetworkCredential(senderEmail, senderPassword),
            EnableSsl = true,  // Enable SSL for security
        };

        MailMessage mailMessage = new MailMessage
        {
            From = new MailAddress(senderEmail),
            Subject = subject,
            Body = body,
        };

        mailMessage.To.Add(to);

        try
        {
            smtpClient.Send(mailMessage);
            Console.WriteLine("Email sent successfully!");
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // Handle any errors
            Console.WriteLine("Failed to send email: " + ex.Message);
            return Task.CompletedTask;
        }

    }
}
