using SendGrid;
using SendGrid.Helpers.Mail;
using TraceWebApi.Interfaces;

namespace TraceWebApi.Services.EmailProvider;

public class SendGridService : IEmailService
{
    private readonly IConfiguration _configuration;

    public SendGridService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {

        string emailApiKey = _configuration["Azure:SendGridKey"]!;
        string email = _configuration["TraceAI:Email"]!;
        var client = new SendGridClient(emailApiKey);

        var message = new SendGridMessage
        {
            From = new EmailAddress(email),
            Subject = subject,
            HtmlContent = body
        };
        message.AddTo(new EmailAddress(to));
        var response = await client.SendEmailAsync(message);


        Console.Write(response.ToString());
    }   
}
