using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RiskPortal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RiskPortal.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // --- REINICIO FORZADO DE BASE DE DATOS ---
            // 1. Obligamos a SQLite a soltar el archivo para evitar el error de bloqueo
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            
            // 2. Destruimos cualquier base de datos residual
            await context.Database.EnsureDeletedAsync();
            
            // 3. Creamos todo desde cero sin depender de las migraciones
            await context.Database.EnsureCreatedAsync();
            // -----------------------------------------

            // 1. Crear Rol Analista
            if (!await roleManager.RoleExistsAsync("Analista"))
            {
                await roleManager.CreateAsync(new IdentityRole("Analista"));
            }

            // 2. Crear Usuario Analista
            var analistaEmail = "analista@riskportal.com";
            var analistaUser = await userManager.FindByEmailAsync(analistaEmail);
            if (analistaUser == null)
            {
                analistaUser = new IdentityUser { UserName = analistaEmail, Email = analistaEmail };
                await userManager.CreateAsync(analistaUser, "Password123!");
                await userManager.AddToRoleAsync(analistaUser, "Analista");
            }

            // 3. Crear Clientes y Solicitudes
            if (!context.Clientes.Any())
            {
                var clientes = new List<Cliente>
                {
                    new Cliente { UsuarioId = "cliente1@riskportal.com", IngresosMensuales = 3500.00m, Activo = true },
                    new Cliente { UsuarioId = "cliente2@riskportal.com", IngresosMensuales = 5000.00m, Activo = true },
                    new Cliente { UsuarioId = "cliente3@riskportal.com", IngresosMensuales = 8200.00m, Activo = true },
                    new Cliente { UsuarioId = "cliente4@riskportal.com", IngresosMensuales = 1500.00m, Activo = true },
                    new Cliente { UsuarioId = "cliente5@riskportal.com", IngresosMensuales = 12000.00m, Activo = true }
                };

                context.Clientes.AddRange(clientes);
                await context.SaveChangesAsync();

                var solicitudes = new List<SolicitudCredito>
                {
                    // Cliente 1
                    new SolicitudCredito { ClienteId = clientes[0].Id, MontoSolicitado = 10000.00m, FechaSolicitud = DateTime.UtcNow.AddDays(-2), Estado = EstadoSolicitud.Pendiente },
                    // Cliente 2
                    new SolicitudCredito { ClienteId = clientes[1].Id, MontoSolicitado = 15000.00m, FechaSolicitud = DateTime.UtcNow.AddDays(-5), Estado = EstadoSolicitud.Aprobado },
                    new SolicitudCredito { ClienteId = clientes[1].Id, MontoSolicitado = 3000.00m, FechaSolicitud = DateTime.UtcNow.AddDays(-30), Estado = EstadoSolicitud.Aprobado },
                    // Cliente 3
                    new SolicitudCredito { ClienteId = clientes[2].Id, MontoSolicitado = 40000.00m, FechaSolicitud = DateTime.UtcNow.AddDays(-10), Estado = EstadoSolicitud.Aprobado },
                    new SolicitudCredito { ClienteId = clientes[2].Id, MontoSolicitado = 5000.00m, FechaSolicitud = DateTime.UtcNow.AddDays(-20), Estado = EstadoSolicitud.Rechazado, MotivoRechazo = "Historial crediticio insuficiente en su momento." },
                    // Cliente 4
                    new SolicitudCredito { ClienteId = clientes[3].Id, MontoSolicitado = 8000.00m, FechaSolicitud = DateTime.UtcNow.AddDays(-1), Estado = EstadoSolicitud.Rechazado, MotivoRechazo = "El monto excede su capacidad de pago (5x ingresos)." },
                    // Cliente 5
                    new SolicitudCredito { ClienteId = clientes[4].Id, MontoSolicitado = 50000.00m, FechaSolicitud = DateTime.UtcNow.AddDays(-15), Estado = EstadoSolicitud.Aprobado },
                    new SolicitudCredito { ClienteId = clientes[4].Id, MontoSolicitado = 12000.00m, FechaSolicitud = DateTime.UtcNow, Estado = EstadoSolicitud.Pendiente }
                };

                context.Solicitudes.AddRange(solicitudes);
                await context.SaveChangesAsync();
            }
        }
    }
}