# ConsumerService

Repositorio de servicios de Windows desarrollados en .NET 8.

## Proyectos

### ?? [UrlPoller](./UrlPoller/README.md)
**Windows Service para Consulta Periódica de URLs**

Servicio que realiza polling HTTP periódico a una URL configurada y registra los resultados.

- ? Ejecutable como Windows Service (nombre: `HealtyRabbit`)
- ? Consulta HTTP cada 8 minutos (configurable)
- ? Registro de StatusCode y tiempo de respuesta
- ? Manejo robusto de errores
- ? Configuración mediante `appsettings.json`
- ? Logging compatible con Windows Event Log

[?? Ver documentación completa](./UrlPoller/README.md)

## Requisitos

- .NET 8 SDK
- Windows (para ejecutar como servicio)
- PowerShell (para instalación)

## Instalación Rápida

```powershell
# Clonar el repositorio
git clone https://github.com/feliper557/ConsumerService.git
cd ConsumerService

# Navegar al proyecto deseado
cd UrlPoller

# Publicar
dotnet publish -c Release -r win-x64 -o publish

# Instalar como servicio (PowerShell como Administrador)
sc.exe create HealtyRabbit binPath= "C:\RUTA\COMPLETA\publish\UrlPoller.exe" start= auto obj= LocalSystem
sc.exe description HealtyRabbit "Consulta periódicamente una URL"
sc.exe start HealtyRabbit
```

## Estructura del Repositorio

```
ConsumerService/
??? .gitignore
??? README.md
??? UrlPoller/                  # Proyecto de polling HTTP
    ??? Program.cs
    ??? HealtyRabbitWorker.cs
    ??? PollingOptions.cs
    ??? appsettings.json
    ??? UrlPoller.csproj
    ??? README.md
```

## Contribuir

1. Fork el proyecto
2. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

## Licencia

Este proyecto es de código abierto.

## Autor

[@feliper557](https://github.com/feliper557)
