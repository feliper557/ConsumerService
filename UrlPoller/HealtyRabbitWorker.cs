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
        
        // Solo log al iniciar el servicio
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
            // Detención normal del servicio - no loggear nada
        }
        catch (Exception ex)
        {
            // Log solo de errores críticos inesperados
            _logger.LogCritical(ex, "Error crítico en el ciclo principal del worker");
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
            // Obtener el cliente HTTP nombrado
            var httpClient = _httpClientFactory.CreateClient("poller");

            // Realizar la petición GET
            var response = await httpClient.GetAsync(_options.Url, cancellationToken);
            
            stopwatch.Stop();

            // Solo loggear si hay error (no exitoso)
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Error en respuesta - StatusCode: {StatusCode} {ReasonPhrase} - URL: {Url} - Tiempo: {ElapsedMs}ms",
                    (int)response.StatusCode,
                    response.ReasonPhrase,
                    _options.Url,
                    stopwatch.ElapsedMilliseconds);
            }
            // Si es exitoso, no loggear nada
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
            
            // Solo loggear timeout, no cancelación normal del servicio
            if (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(
                    ex,
                    "Timeout al consultar {Url} - Tiempo: {ElapsedMs}ms",
                    _options.Url,
                    stopwatch.ElapsedMilliseconds);
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
        // Solo log al detener el servicio
        _logger.LogInformation("HealtyRabbitWorker detenido");
        return base.StopAsync(cancellationToken);
    }
}
