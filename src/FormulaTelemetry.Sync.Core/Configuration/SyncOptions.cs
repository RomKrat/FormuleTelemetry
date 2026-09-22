namespace FormulaTelemetry.Sync.Core.Configuration;

public sealed class OpenF1Options
{
    public const string SectionName = "OpenF1";

    public string BaseUrl { get; set; } = "https://api.openf1.org/v1/";
}

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    public string ConnectionString { get; set; } = string.Empty;
}

public sealed class SyncWorkerOptions
{
    public const string SectionName = "SyncWorker";

    /// <summary>How often the worker discovers and syncs sessions.</summary>
    public int IntervalMinutes { get; set; } = 15;

    /// <summary>When true, run auto-discover once immediately at startup.</summary>
    public bool RunPendingOnStartup { get; set; } = true;

    /// <summary>Also scan this many previous calendar years (1 = current + previous).</summary>
    public int DiscoverYearsBack { get; set; } = 1;

    /// <summary>Pause between sessions during discover (helps avoid OpenF1 429).</summary>
    public int PauseBetweenSessionsSeconds { get; set; } = 2;
}

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    public bool Enabled { get; set; }

    public string SmtpHost { get; set; } = string.Empty;

    public int SmtpPort { get; set; } = 587;

    public bool UseSsl { get; set; } = true;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    /// <summary>Sender address.</summary>
    public string From { get; set; } = string.Empty;

    /// <summary>Recipient(s), comma or semicolon separated.</summary>
    public string To { get; set; } = string.Empty;
}
