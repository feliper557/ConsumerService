using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace UrlPoller;

/// <summary>
/// Worker que consulta periódicamente una URL y registra el resultado
/// </summary>
public class HealtyRabbitWorker : BackgroundService
{
    private readonly ILogger<HealtyRabbitWorker> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly PollingOptions _options;

    public HealtyRabbitWorker(
        ILogger<HealtyRabbitWorker> logger,
        IHttpClientFactory httpClientFactory,
        IOptions<PollingOptions> options)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Validar y ajustar el intervalo
        var intervalMinutes = _options.IntervalMinutes < 1 ? 1 : _options.IntervalMinutes;
        
        _logger.LogInformation(
            "HealtyRabbitWorker iniciado. URL: {Url}, Intervalo: {Interval} minutos",
            _options.Url,
            intervalMinutes);

        // Crear el PeriodicTimer con el intervalo configurado
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(intervalMinutes));

        try
        {
            // El bucle se ejecutará cada vez que el timer se active
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await PollUrlAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Esto es esperado cuando se cancela el servicio
            _logger.LogInformation("HealtyRabbitWorker detenido por cancelación");
        }
        catch (Exception ex)
        {
            // Log de errores inesperados, pero no crashear el servicio
            _logger.LogError(ex, "Error inesperado en el ciclo principal del worker");
        }
    }

    /// <summary>
    /// Realiza la consulta HTTP a la URL configurada
    /// </summary>
    private async Task PollUrlAsync(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Iniciando consulta a {Url}", _options.Url);

            // Obtener el cliente HTTP nombrado
            var httpClient = _httpClientFactory.CreateClient("poller");

            // Realizar la petición GET
            var response = await httpClient.GetAsync(_options.Url, cancellationToken);
            
            stopwatch.Stop();

            // Registrar el resultado
            _logger.LogInformation(
                "Respuesta recibida - StatusCode: {StatusCode}, Tiempo: {ElapsedMs}ms",
                (int)response.StatusCode,
                stopwatch.ElapsedMilliseconds);

            // Si no fue exitoso, registrar un warning adicional
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "La respuesta no fue exitosa: {StatusCode} {ReasonPhrase} {Url}",
                    (int)response.StatusCode,
                    response.ReasonPhrase,
                    _options.Url);
            }
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "Error HTTP al consultar {Url} - Tiempo: {ElapsedMs}ms",
                _options.Url,
                stopwatch.ElapsedMilliseconds);
        }
        catch (TaskCanceledException ex)
        {
            stopwatch.Stop();
            
            // Distinguir entre timeout y cancelación por token
            if (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(
                    ex,
                    "Timeout al consultar {Url} - Tiempo: {ElapsedMs}ms",
                    _options.Url,
                    stopwatch.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogInformation("Consulta cancelada por detención del servicio");
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "Error inesperado al consultar {Url} - Tiempo: {ElapsedMs}ms",
                _options.Url,
                stopwatch.ElapsedMilliseconds);
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("HealtyRabbitWorker finalizando...");
        return base.StopAsync(cancellationToken);
    }
}
