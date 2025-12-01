using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TraceWebApi.Interfaces;

namespace TraceWebApi.Services.EmailProvider;

public class MLSNEmailService : IEmailService
{
    private static readonly string ApiKey = "mlsn.f3e066cf1296748ea7a3196715aa19672fe9e2be451f998c56ed9846b85c2ffd";
    private static readonly string ApiUrl = "https://api.mailersend.com/v1/email";

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var emailData = new
        {
            from = new { email = "traceaidevelopment@gmail.com", name = "Trace AI" },
            to = new[] { new { email = to, name = "Test" } },
            subject,
            text = body,
            html = $"<p>body</p>"
        };

        string json = JsonSerializer.Serialize(emailData);
        StringContent content = new(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.PostAsync(ApiUrl, content);

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Email sent successfully!");
        }
        else
        {
            string errorMessage = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error: {errorMessage}");
        }
    }
}
