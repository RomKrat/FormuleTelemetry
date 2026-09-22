using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FormulaTelemetry.Sync.Core.Configuration;

namespace FormulaTelemetry.Sync.Core.OpenF1;

public interface IOpenF1Client
{
    Task<IReadOnlyList<T>> GetAsync<T>(string relativePath, CancellationToken cancellationToken = default);
}

public sealed class OpenF1Client : IOpenF1Client
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenF1Client> _logger;

    public OpenF1Client(HttpClient httpClient, IOptions<OpenF1Options> options, ILogger<OpenF1Client> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        var baseUrl = options.Value.BaseUrl;
        if (!baseUrl.EndsWith('/'))
        {
            baseUrl += "/";
        }

        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.Timeout = TimeSpan.FromMinutes(5);
    }

    public async Task<IReadOnlyList<T>> GetAsync<T>(string relativePath, CancellationToken cancellationToken = default)
    {
        const int maxAttempts = 8;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            _logger.LogInformation("OpenF1 GET {Path} (attempt {Attempt})", relativePath, attempt);

            using var response = await _httpClient.GetAsync(relativePath, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests && attempt < maxAttempts)
            {
                var delay = TimeSpan.FromSeconds(Math.Min(60, Math.Pow(2, attempt + 1)));
                _logger.LogWarning("OpenF1 rate limited (429). Waiting {Delay}s…", delay.TotalSeconds);
                await Task.Delay(delay, cancellationToken);
                continue;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogInformation("OpenF1 GET {Path} returned 404 — treating as empty.", relativePath);
                return [];
            }

            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadFromJsonAsync<List<T>>(JsonOptions, cancellationToken);
            return data ?? [];
        }

        throw new HttpRequestException($"OpenF1 GET {relativePath} failed after {maxAttempts} attempts.");
    }
}
