# UrlPoller - Windows Service para Consulta Periódica de URLs

Servicio de Windows (.NET 8) que realiza consultas HTTP periódicas a una URL configurada y registra los resultados.

## Características

- ? Ejecutable como **Windows Service** con nombre `HealtyRabbit`
- ? Consulta HTTP cada **8 minutos** (configurable) usando `PeriodicTimer`
- ? Registro de **StatusCode** y **tiempo de respuesta**
- ? Manejo robusto de errores (timeout, DNS, excepciones HTTP)
- ? Configuración mediante `appsettings.json`
- ? Logging compatible con **Windows Event Log**
- ? Arquitectura moderna con `IHttpClientFactory` y opciones tipadas

## Configuración

Edita `appsettings.json` para configurar la URL y el intervalo:

```json
{
  "Polling": {
    "Url": "https://example.com/health",
    "IntervalMinutes": 8
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

**Nota:** Si `IntervalMinutes` es menor a 1, se usará 1 minuto como mínimo.

## Estructura del Proyecto

```
UrlPoller/
??? Program.cs                  # Configuración del host y servicios
??? HealtyRabbitWorker.cs       # BackgroundService con lógica de polling
??? PollingOptions.cs           # POCO para opciones de configuración
??? appsettings.json            # Configuración de la aplicación
??? UrlPoller.csproj            # Archivo del proyecto .NET 8
```

## Compilación y Publicación

### 1. Publicar para Windows (x64)

```powershell
cd UrlPoller
dotnet publish -c Release -r win-x64 -o publish
```

Esto generará los binarios en la carpeta `publish\`.

### 2. Publicar como archivo único (opcional)

Para generar un único ejecutable, descomenta las opciones en `UrlPoller.csproj`:

```xml
<PublishSingleFile>true</PublishSingleFile>
<SelfContained>true</SelfContained>
<RuntimeIdentifier>win-x64</RuntimeIdentifier>
```

Luego ejecuta:

```powershell
dotnet publish -c Release
```

## Instalación como Windows Service

### Requisitos
- Ejecutar los comandos en **PowerShell como Administrador**
- .NET 8 Runtime instalado (si no usas self-contained)

### 1. Crear el servicio

```powershell
sc.exe create HealtyRabbit binPath= "C:\RUTA\COMPLETA\publish\UrlPoller.exe" start= auto
```

> ?? **Importante:** Reemplaza `C:\RUTA\COMPLETA\publish\` con la ruta real donde publicaste el proyecto.  
> ?? **Nota:** Debe haber un espacio después de `binPath=` y `start=`

### 2. Agregar descripción al servicio

```powershell
sc.exe description HealtyRabbit "Consulta periódicamente una URL y registra el estado de la respuesta"
```

### 3. Iniciar el servicio

```powershell
sc.exe start HealtyRabbit
```

### 4. Verificar el estado

```powershell
sc.exe query HealtyRabbit
```

O abre `services.msc` y busca **HealtyRabbit** en la lista.

## Gestión del Servicio

### Detener el servicio

```powershell
sc.exe stop HealtyRabbit
```

### Eliminar el servicio

```powershell
sc.exe delete HealtyRabbit
```

> ?? Debes detener el servicio antes de eliminarlo.

### Ver logs en Event Viewer

1. Abre **Visor de eventos** (Event Viewer)
2. Ve a: `Windows Logs` ? `Application`
3. Filtra por origen: `.NET Runtime` o busca eventos de `HealtyRabbit`

## Arquitectura Técnica

### Componentes Principales

#### `Program.cs`
- Configura el **Host genérico** de .NET
- Habilita ejecución como **Windows Service** con `AddWindowsService()`
- Registra `IHttpClientFactory` con cliente nombrado `"poller"`
- Configura opciones tipadas mediante `IOptions<PollingOptions>`
- Registra `HealtyRabbitWorker` como `BackgroundService`

#### `HealtyRabbitWorker.cs`
- Implementa `BackgroundService`
- Usa `PeriodicTimer` para ejecución cada N minutos (sin solapes)
- Maneja excepciones de red, timeout y errores HTTP
- Registra StatusCode y tiempo de respuesta en logs
- Finaliza limpiamente cuando se recibe `CancellationToken`

#### `PollingOptions.cs`
- POCO (Plain Old CLR Object) para configuración
- Propiedades: `Url` (string) y `IntervalMinutes` (int)

### Características de Resiliencia

- ? **No crashea** ante errores de red o HTTP
- ?? **Reintenta** automáticamente en el siguiente ciclo
- ?? **Timeout configurado** (5 minutos por defecto)
- ?? **Logging detallado** de cada operación
- ?? **Cancelación limpia** mediante `CancellationToken`

## Ejemplos de Logs

### Inicio del servicio
```
[Information] HealtyRabbitWorker iniciado. URL: https://example.com/health, Intervalo: 8 minutos
```

### Consulta exitosa
```
[Information] Iniciando consulta a https://example.com/health
[Information] Respuesta recibida - StatusCode: 200, Tiempo: 234ms
```

### Error de timeout
```
[Error] Timeout al consultar https://example.com/health - Tiempo: 5000ms
```

### Detención del servicio
```
[Information] HealtyRabbitWorker finalizando...
[Information] HealtyRabbitWorker detenido por cancelación
```

## Troubleshooting

### El servicio no inicia

1. Verifica que la ruta del `binPath` sea correcta y absoluta
2. Asegúrate de que `appsettings.json` esté en la misma carpeta que el ejecutable
3. Revisa el Event Viewer para ver mensajes de error

### No veo logs

- Los logs se escriben en el **Event Log de Windows** (Application)
- Asegúrate de tener permisos para escribir en el Event Log
- Ajusta el nivel de log en `appsettings.json` si es necesario

### Cambiar la URL sin reinstalar

1. Edita `appsettings.json` en la carpeta de publicación
2. Reinicia el servicio: `sc.exe stop HealtyRabbit && sc.exe start HealtyRabbit`

## Desarrollo y Testing

### Ejecutar en modo consola (desarrollo)

```powershell
cd UrlPoller
dotnet run
```

Esto ejecutará el worker como aplicación de consola, útil para debugging.

### Ejecutar tests

```powershell
dotnet test
```

## Licencia

Este proyecto es de código abierto. Siéntete libre de usar, modificar y distribuir.

## Autor

Desarrollado con ?? usando .NET 8 y Worker Service template.
