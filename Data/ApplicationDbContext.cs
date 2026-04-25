using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RiskPortal.Models;

namespace RiskPortal.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<SolicitudCredito> Solicitudes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Restricción: Un cliente solo puede tener UNA solicitud en estado Pendiente (Pendiente = 0)
            builder.Entity<SolicitudCredito>()
                .HasIndex(s => s.ClienteId)
                .IsUnique()
                .HasFilter("\"Estado\" = 0");

            // Check Constraints para obligar valores mayores a 0 en la BD
            builder.Entity<Cliente>()
                .ToTable(t => t.HasCheckConstraint("CK_Cliente_Ingresos", "\"IngresosMensuales\" > 0"));
            builder.Entity<SolicitudCredito>()
                .ToTable(t => t.HasCheckConstraint("CK_Solicitud_Monto", "\"MontoSolicitado\" > 0"));
        }
    }
}
