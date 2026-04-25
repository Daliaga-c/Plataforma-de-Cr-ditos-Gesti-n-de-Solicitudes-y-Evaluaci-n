using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RiskPortal.Data;
using RiskPortal.Models;
using System.Linq;
using System.Threading.Tasks;

namespace RiskPortal.Controllers
{
    [Authorize] // Requiere que el usuario esté autenticado
    public class SolicitudesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SolicitudesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Solicitudes
        public async Task<IActionResult> Index(SolicitudFilterViewModel filter)
        {
            // Validar que no haya montos negativos ni fechas incongruentes
            if (!ModelState.IsValid)
            {
                filter.Solicitudes = new List<SolicitudCredito>();
                return View(filter);
            }

            var query = _context.Solicitudes.Include(s => s.Cliente).AsQueryable();

            // Requisito: "listado de solicitudes del usuario autenticado"
            // Si el usuario NO es un Analista, filtramos solo sus propias solicitudes.
            var userId = User.Identity?.Name;
            if (!User.IsInRole("Analista"))
            {
                query = query.Where(s => s.Cliente != null && s.Cliente.UsuarioId == userId);
            }

            // Aplicar Filtros
            if (filter.Estado.HasValue)
                query = query.Where(s => s.Estado == filter.Estado.Value);

            if (filter.MontoMinimo.HasValue)
                query = query.Where(s => s.MontoSolicitado >= filter.MontoMinimo.Value);

            if (filter.MontoMaximo.HasValue)
                query = query.Where(s => s.MontoSolicitado <= filter.MontoMaximo.Value);

            if (filter.FechaInicio.HasValue)
                query = query.Where(s => s.FechaSolicitud >= filter.FechaInicio.Value);

            if (filter.FechaFin.HasValue)
            {
                var end = filter.FechaFin.Value.AddDays(1);
                query = query.Where(s => s.FechaSolicitud < end);
            }

            filter.Solicitudes = await query.OrderByDescending(s => s.FechaSolicitud).ToListAsync();

            return View(filter);
        }

        // GET: Solicitudes/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var solicitud = await _context.Solicitudes
                .Include(s => s.Cliente)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (solicitud == null) return NotFound();

            return View(solicitud);
        }
    }
}