using System.ComponentModel.DataAnnotations;

namespace RiskPortal.Models
{
    public class CreateSolicitudViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar un cliente.")]
        [Display(Name = "Cliente")]
        public int ClienteId { get; set; }

        [Required(ErrorMessage = "El monto solicitado es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto solicitado debe ser mayor a 0.")]
        [Display(Name = "Monto Solicitado")]
        public decimal MontoSolicitado { get; set; }
    }
}