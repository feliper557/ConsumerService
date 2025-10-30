namespace UrlPoller;

/// <summary>
/// Opciones de configuración para el polling de URL
/// </summary>
public class PollingOptions
{
    /// <summary>
    /// Sección de configuración en appsettings.json
    /// </summary>
    public const string Polling = "Polling";

    /// <summary>
    /// URL a consultar periódicamente
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Intervalo de consulta en minutos (mínimo 1)
    /// </summary>
    public int IntervalMinutes { get; set; } = 8;
}
