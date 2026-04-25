using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using RiskPortal.Data;
using RiskPortal.Models;
using System.Linq;
using System.Threading.Tasks;

namespace RiskPortal.Controllers
{
    [Authorize(Roles = "Analista")]
    public class AnalistaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;

        public AnalistaController(ApplicationDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // GET: /Analista
        public async Task<IActionResult> Index()
        {
            // Solo mostramos solicitudes en estado Pendiente
            var pendientes = await _context.Solicitudes
                .Include(s => s.Cliente)
                .Where(s => s.Estado == EstadoSolicitud.Pendiente)
                .OrderBy(s => s.FechaSolicitud)
                .ToListAsync();

            return View(pendientes);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aprobar(int id)
        {
            var solicitud = await _context.Solicitudes.Include(s => s.Cliente).FirstOrDefaultAsync(s => s.Id == id);
            if (solicitud == null) return NotFound();

            if (solicitud.Estado != EstadoSolicitud.Pendiente)
                return BadRequest("Solo se pueden procesar solicitudes pendientes.");

            if (!solicitud.CumpleCapacidadPago())
            {
                TempData["ErrorMessage"] = $"No se puede aprobar. El monto ({solicitud.MontoSolicitado:C}) excede 5 veces los ingresos del cliente.";
                return RedirectToAction(nameof(Index));
            }

            solicitud.Estado = EstadoSolicitud.Aprobado;
            await _context.SaveChangesAsync();
            await InvalidarCache(solicitud);

            TempData["SuccessMessage"] = $"Solicitud #REQ-{solicitud.Id:D4} aprobada exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rechazar(int id, string motivoRechazo)
        {
            var solicitud = await _context.Solicitudes.Include(s => s.Cliente).FirstOrDefaultAsync(s => s.Id == id);
            if (solicitud == null) return NotFound();

            if (solicitud.Estado != EstadoSolicitud.Pendiente)
                return BadRequest("Solo se pueden procesar solicitudes pendientes.");

            if (string.IsNullOrWhiteSpace(motivoRechazo))
            {
                TempData["ErrorMessage"] = "El motivo de rechazo es obligatorio.";
                return RedirectToAction(nameof(Index));
            }

            solicitud.Estado = EstadoSolicitud.Rechazado;
            solicitud.MotivoRechazo = motivoRechazo;
            await _context.SaveChangesAsync();
            await InvalidarCache(solicitud);

            TempData["SuccessMessage"] = $"Solicitud #REQ-{solicitud.Id:D4} rechazada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        private async Task InvalidarCache(SolicitudCredito solicitud)
        {
            await _cache.RemoveAsync($"solicitudes_{User.Identity?.Name ?? "anonymous"}");
            if (solicitud.Cliente?.UsuarioId != null)
                await _cache.RemoveAsync($"solicitudes_{solicitud.Cliente.UsuarioId}");
        }
    }
}