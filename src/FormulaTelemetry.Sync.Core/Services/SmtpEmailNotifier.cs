using System.Net;
using System.Net.Mail;
using FormulaTelemetry.Sync.Core.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FormulaTelemetry.Sync.Core.Services;

public sealed class SmtpEmailNotifier : IEmailNotifier
{
    private readonly EmailOptions _options;
    private readonly ILogger<SmtpEmailNotifier> _logger;

    public SmtpEmailNotifier(IOptions<EmailOptions> options, ILogger<SmtpEmailNotifier> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(string subject, string body, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            _logger.LogDebug("Email disabled; skip send: {Subject}", subject);
            return;
        }

        if (string.IsNullOrWhiteSpace(_options.SmtpHost) ||
            string.IsNullOrWhiteSpace(_options.From) ||
            string.IsNullOrWhiteSpace(_options.To))
        {
            _logger.LogWarning("Email enabled but SmtpHost/From/To incomplete; skip send.");
            return;
        }

        using var message = new MailMessage
        {
            From = new MailAddress(_options.From.Trim()),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };

        foreach (var recipient in SplitAddresses(_options.To))
        {
            message.To.Add(recipient);
        }

        if (message.To.Count == 0)
        {
            _logger.LogWarning("Email To has no valid addresses; skip send.");
            return;
        }

        using var client = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
        {
            EnableSsl = _options.UseSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        if (!string.IsNullOrWhiteSpace(_options.Username))
        {
            client.Credentials = new NetworkCredential(_options.Username, _options.Password);
        }

        cancellationToken.ThrowIfCancellationRequested();
        await client.SendMailAsync(message, cancellationToken);
        _logger.LogInformation("Email sent: {Subject} → {To}", subject, _options.To);
    }

    private static IEnumerable<string> SplitAddresses(string raw) =>
        raw.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(a => a.Length > 0);
}
