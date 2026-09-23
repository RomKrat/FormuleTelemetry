namespace FormulaTelemetry.Web.Helpers;

/// <summary>Country → ISO 3166-1 alpha-2 for flag assets (not official IP).</summary>
public static class CountryFlags
{
    private static readonly Dictionary<string, string> ByName = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Australia"] = "au",
        ["Austria"] = "at",
        ["Azerbaijan"] = "az",
        ["Bahrain"] = "bh",
        ["Belgium"] = "be",
        ["Brazil"] = "br",
        ["Canada"] = "ca",
        ["China"] = "cn",
        ["France"] = "fr",
        ["Germany"] = "de",
        ["Great Britain"] = "gb",
        ["United Kingdom"] = "gb",
        ["UK"] = "gb",
        ["Hungary"] = "hu",
        ["Italy"] = "it",
        ["Japan"] = "jp",
        ["Mexico"] = "mx",
        ["Monaco"] = "mc",
        ["Netherlands"] = "nl",
        ["Holland"] = "nl",
        ["Qatar"] = "qa",
        ["Saudi Arabia"] = "sa",
        ["Singapore"] = "sg",
        ["Spain"] = "es",
        ["United States"] = "us",
        ["USA"] = "us",
        ["United Arab Emirates"] = "ae",
        ["UAE"] = "ae",
        ["Abu Dhabi"] = "ae",
        ["Portugal"] = "pt",
        ["Russia"] = "ru",
        ["Turkey"] = "tr",
        ["South Korea"] = "kr",
        ["Korea"] = "kr",
        ["India"] = "in",
        ["Malaysia"] = "my",
        ["Argentina"] = "ar",
        ["South Africa"] = "za",
        ["Switzerland"] = "ch",
    };

    public static string? Iso2(string? countryName)
    {
        if (string.IsNullOrWhiteSpace(countryName)) return null;
        if (ByName.TryGetValue(countryName.Trim(), out var code)) return code;

        // Partial match: "Emilia-Romagna" stays Italy via location, not country — skip.
        foreach (var (name, iso) in ByName)
        {
            if (countryName.Contains(name, StringComparison.OrdinalIgnoreCase))
            {
                return iso;
            }
        }

        return null;
    }

    public static string? FlagUrl(string? countryName, int width = 40)
    {
        var iso = Iso2(countryName);
        return iso is null ? null : $"https://flagcdn.com/w{width}/{iso}.png";
    }
}
