using System.Net.Http.Json;
using CvManagementSystem.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace CvManagementSystem.Infrastructure.Email;

public class BrevoEmailSender : IEmailSender
{
    private readonly HttpClient _httpClient;
    private readonly IOptions<BrevoOptions> _options;

    public BrevoEmailSender(HttpClient httpClient, IOptions<BrevoOptions> options)
    {
        _httpClient = httpClient;
        _options = options;
    }

    public async Task SendAsync(string recipient, string subject, string body, CancellationToken cancellationToken = default)
    {
        var settings = _options.Value;
        using var request = new HttpRequestMessage(HttpMethod.Post, "v3/smtp/email");
        request.Headers.Add("api-key", settings.ApiKey);
        request.Headers.Accept.ParseAdd("application/json");
        request.Content = JsonContent.Create(new
        {
            sender = new { email = settings.FromAddress, name = settings.FromName },
            to = new[] { new { email = recipient } },
            subject,
            textContent = body
        });

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
