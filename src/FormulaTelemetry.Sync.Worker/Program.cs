using FormulaTelemetry.Sync.Core;
using FormulaTelemetry.Sync.Worker;

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "FormulaTelemetry.Sync";
});

builder.Services.ConfigureFormulaTelemetrySync(builder.Configuration);
builder.Services.AddFormulaTelemetrySync();
builder.Services.AddHostedService<SyncPendingWorker>();

var host = builder.Build();
host.Run();
