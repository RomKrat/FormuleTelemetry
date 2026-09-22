namespace FormulaTelemetry.Sync.Core.Services;

public interface IEmailNotifier
{
    Task SendAsync(string subject, string body, CancellationToken cancellationToken = default);
}
