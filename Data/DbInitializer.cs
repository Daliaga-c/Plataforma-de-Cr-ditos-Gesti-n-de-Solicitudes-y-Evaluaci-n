using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RiskPortal.Models;
using System;
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

            // Asegura que la BD esté creada y migrada
            await context.Database.MigrateAsync();

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
                var cliente1 = new Cliente
                {
                    UsuarioId = "user-cli-1", // Referencia abstracta si el cliente no usa Identity aún
                    IngresosMensuales = 3500.00m,
                    Activo = true
                };

                var cliente2 = new Cliente
                {
                    UsuarioId = "user-cli-2",
                    IngresosMensuales = 5000.00m,
                    Activo = true
                };

                context.Clientes.AddRange(cliente1, cliente2);
                await context.SaveChangesAsync();

                var solicitud1 = new SolicitudCredito
                {
                    ClienteId = cliente1.Id,
                    MontoSolicitado = 10000.00m,
                    FechaSolicitud = DateTime.UtcNow.AddDays(-2),
                    Estado = EstadoSolicitud.Pendiente
                };

                var solicitud2 = new SolicitudCredito
                {
                    ClienteId = cliente2.Id,
                    MontoSolicitado = 15000.00m, // Cumple regla: 15,000 <= 5000 * 5
                    FechaSolicitud = DateTime.UtcNow.AddDays(-5),
                    Estado = EstadoSolicitud.Aprobado
                };

                context.Solicitudes.AddRange(solicitud1, solicitud2);
                await context.SaveChangesAsync();
            }
        }
    }
}