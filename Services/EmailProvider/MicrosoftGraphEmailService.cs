using Azure.Core;
using Microsoft.Identity.Client;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TraceWebApi.Interfaces;

namespace TraceWebApi.Services.EmailProvider;

public class MicrosoftGraphEmailService : IEmailService
{

    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly string _accessToken;

    public MicrosoftGraphEmailService(
        IConfiguration configuration)
    {
        _configuration = configuration;
        _httpClient = new HttpClient();
        _accessToken = GetAccessTokenAsync().Result;
    }
    private async Task<string> GetAccessTokenAsync()
    {
        var clientId = _configuration["AzureAd:ClientId"];
        var tenantId = _configuration["AzureAd:TenantId"];
        var clientSecret = _configuration["AzureAd:ClientSecret"];

        var app = ConfidentialClientApplicationBuilder
            .Create(clientId)
            .WithClientSecret(clientSecret)
            .WithAuthority($"https://login.microsoftonline.com/{tenantId}")
            .Build();

        var result = await app.AcquireTokenForClient(new[] { "https://graph.microsoft.com/.default" }).ExecuteAsync();
        return result.AccessToken;
    }


    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var emailData = new
        {
            Message = new
            {
                Subject = subject,
                Body = new
                {
                    ContentType = "HTML",
                    Content = body
                },
                ToRecipients = new[]
                {
                    new { EmailAddress = new { Address = to } }
                }
            },
            SaveToSentItems = true
        };

        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(emailData), Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync("https://graph.microsoft.com/v1.0/users/support@traceaidevelopmentgmail.onmicrosoft.com/sendMail", content);

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Email sent successfully.");
        }
        else
        {
            Console.WriteLine($"Error: {response.StatusCode}");
        }

        response.EnsureSuccessStatusCode();
    }
}
