using System;
using System.ComponentModel.DataAnnotations;

namespace RiskPortal.Models
{
    public class SolicitudCredito
    {
        public int Id { get; set; }
        
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }
        
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto solicitado debe ser mayor a 0.")]
        public decimal MontoSolicitado { get; set; }
        
        public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow;
        
        public EstadoSolicitud Estado { get; set; } = EstadoSolicitud.Pendiente;
        
        public string? MotivoRechazo { get; set; }

        // Regla de Negocio: No se puede aprobar si el monto > 5 veces los ingresos
        public bool CumpleCapacidadPago()
        {
            if (Cliente == null) throw new InvalidOperationException("La información del cliente no está cargada.");
            
            return MontoSolicitado <= (Cliente.IngresosMensuales * 5);
        }
    }
}