using UrlPoller;

// Crear el builder del host
var builder = Host.CreateApplicationBuilder(args);

// Configurar el servicio para ejecutarse como Windows Service
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "HealtyRabbit";
});

// Registrar HttpClientFactory con cliente nombrado "poller"
builder.Services.AddHttpClient("poller", client =>
{
    // Configuración del cliente HTTP
    client.Timeout = TimeSpan.FromMinutes(5); // Timeout de 5 minutos
    client.DefaultRequestHeaders.Add("User-Agent", "HealtyRabbit-UrlPoller/1.0");
});

// Registrar opciones tipadas desde configuración
builder.Services.Configure<PollingOptions>(
    builder.Configuration.GetSection(PollingOptions.Polling));

// Registrar el BackgroundService
builder.Services.AddHostedService<HealtyRabbitWorker>();

// Construir y ejecutar el host
var host = builder.Build();
host.Run();
