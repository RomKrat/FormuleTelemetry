using FormulaTelemetry.Api.Configuration;
using FormulaTelemetry.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<DatabaseOptions>(builder.Configuration.GetSection(DatabaseOptions.SectionName));
builder.Services.AddSingleton<ITelemetryReadStore, PostgresTelemetryReadStore>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "FormulaTelemetry API",
        Version = "v1",
        Description = "Read-only middle tier over PostgreSQL formulatelemetry (OpenF1 data)."
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("Clients", policy =>
        policy.WithOrigins(
                "https://localhost:7245",
                "http://localhost:5208",
                "https://localhost:7236",
                "http://localhost:5174",
                "https://localhost:7000",
                "https://localhost:5001",
                "http://localhost:5000",
                "http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Clients");
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
