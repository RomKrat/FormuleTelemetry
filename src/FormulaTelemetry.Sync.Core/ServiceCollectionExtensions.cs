using FormulaTelemetry.Sync.Core.Configuration;
using FormulaTelemetry.Sync.Core.OpenF1;
using FormulaTelemetry.Sync.Core.Persistence;
using FormulaTelemetry.Sync.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FormulaTelemetry.Sync.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFormulaTelemetrySync(this IServiceCollection services)
    {
        services.AddHttpClient<IOpenF1Client, OpenF1Client>();
        services.AddSingleton<ITelemetryStore, PostgresTelemetryStore>();
        services.AddSingleton<IOpenF1SyncService, OpenF1SyncService>();
        services.AddSingleton<IEmailNotifier, SmtpEmailNotifier>();
        return services;
    }

    public static IServiceCollection ConfigureFormulaTelemetrySync(
        this IServiceCollection services,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        services.Configure<OpenF1Options>(configuration.GetSection(OpenF1Options.SectionName));
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));
        services.Configure<SyncWorkerOptions>(configuration.GetSection(SyncWorkerOptions.SectionName));
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
        return services;
    }
}
