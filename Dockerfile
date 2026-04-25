# 1. Etapa de compilación (Usa el SDK de .NET)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiamos todos los archivos y restauramos/publicamos
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# 2. Etapa de ejecución (Usa solo el Runtime para que sea más ligero)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Ejecutar como root temporalmente para permitir a SQLite crear la base de datos
USER root

# Exponemos el puerto 8080 (por defecto en .NET 8)
EXPOSE 8080

ENTRYPOINT ["dotnet", "Plataforma de Créditos — Gestión de Solicitudes y Evaluación.dll"]