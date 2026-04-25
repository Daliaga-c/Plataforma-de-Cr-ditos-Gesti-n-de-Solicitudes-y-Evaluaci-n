# RiskPortal - Plataforma de Gestión de Créditos

Plataforma web interna desarrollada en **ASP.NET Core 8 MVC** para gestionar solicitudes de crédito de clientes. Permite registrar créditos, evaluarlos con reglas de negocio estrictas y manejar el caché y sesiones mediante **Redis**.

## Características Principales

- **Autenticación e Identidad**: Sistema de login y roles (Analista / Cliente).
- **Validaciones de Negocio**:
  - Unicidad de solicitudes pendientes.
  - Límite de crédito (Monto <= 5x Ingresos).
- **Catálogo y Filtros**: Búsqueda avanzada por fechas, montos y estados.
- **Panel de Evaluación**: Interfaz exclusiva para Analistas para aprobar o rechazar solicitudes (con motivo obligatorio).
- **Redis Integrado**: Sesiones persistentes (última solicitud visitada) y caché de 60 segundos para el catálogo, con invalidación automática al modificar registros.

## Tecnologías Utilizadas

- .NET 8 / 10 SDK (C#)
- ASP.NET Core MVC
- Entity Framework Core (SQLite)
- StackExchange.Redis (Distributed Cache & Session)
- Bootstrap 5 (UI/UX personalizado)
- Docker (Para despliegue)

## Instrucciones de Despliegue en Render

Este proyecto está configurado para desplegarse como un **Web Service** usando Docker.

### Variables de Entorno Requeridas:

1. `ASPNETCORE_ENVIRONMENT` = `Production`
2. `ASPNETCORE_URLS` = `http://0.0.0.0:${PORT}`
3. `ConnectionStrings__DefaultConnection` = `DataSource=app.db;Cache=Shared`
4. `Redis__ConnectionString` = `<TU_URL_DE_REDIS>,password=<TU_PASSWORD>`

### Credenciales de Prueba (Se generan automáticamente):

- **Rol Analista**: `analista@riskportal.com` | Password: `Password123!`
- **Rol Cliente**: Autenticación simulada mediante creación de datos en `DbInitializer`.

## Consideraciones para Producción

Dado que se utiliza SQLite, en entornos de contenedores efímeros (como el plan gratuito de Render), los datos se reiniciarán al suspenderse la instancia. El `DbInitializer` se encargará de reconstruir las tablas y poblar los datos semilla automáticamente.

---

_Desarrollado para el proyecto de Programación I._
