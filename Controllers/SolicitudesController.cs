using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        // GET: Solicitudes/Create
        public IActionResult Create()
        {
            // Llenamos el dropdown con los clientes para la vista
            ViewBag.Clientes = new SelectList(_context.Clientes, "Id", "UsuarioId");
            return View();
        }

        // POST: Solicitudes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateSolicitudViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Clientes = new SelectList(_context.Clientes, "Id", "UsuarioId", model.ClienteId);
                return View(model);
            }

            var cliente = await _context.Clientes.FindAsync(model.ClienteId);
            if (cliente == null)
            {
                ModelState.AddModelError("", "El cliente seleccionado no existe.");
                ViewBag.Clientes = new SelectList(_context.Clientes, "Id", "UsuarioId", model.ClienteId);
                return View(model);
            }

            // 1. Validación: El cliente debe estar activo
            if (!cliente.Activo)
            {
                ModelState.AddModelError("", "El cliente seleccionado está inactivo y no puede registrar nuevas solicitudes.");
                ViewBag.Clientes = new SelectList(_context.Clientes, "Id", "UsuarioId", model.ClienteId);
                return View(model);
            }

            // 2. Validación: No permitir más de una solicitud Pendiente por cliente
            bool tienePendiente = await _context.Solicitudes
                .AnyAsync(s => s.ClienteId == model.ClienteId && s.Estado == EstadoSolicitud.Pendiente);
            if (tienePendiente)
            {
                ModelState.AddModelError("", "El cliente ya posee una solicitud en estado Pendiente. Debe ser evaluada antes de permitir otra.");
                ViewBag.Clientes = new SelectList(_context.Clientes, "Id", "UsuarioId", model.ClienteId);
                return View(model);
            }

            // 3. Validación: El monto no puede superar 10 veces los ingresos mensuales
            if (model.MontoSolicitado > (cliente.IngresosMensuales * 10))
            {
                ModelState.AddModelError("MontoSolicitado", $"El monto solicitado ({model.MontoSolicitado:C}) supera el límite de 10 veces los ingresos mensuales del cliente ({(cliente.IngresosMensuales * 10):C}).");
                ViewBag.Clientes = new SelectList(_context.Clientes, "Id", "UsuarioId", model.ClienteId);
                return View(model);
            }

            // 4. Registro de Solicitud en estado Pendiente
            var solicitud = new SolicitudCredito { ClienteId = model.ClienteId, MontoSolicitado = model.MontoSolicitado, FechaSolicitud = DateTime.UtcNow, Estado = EstadoSolicitud.Pendiente };
            _context.Solicitudes.Add(solicitud);
            await _context.SaveChangesAsync();

            // 5. Feedback Claro en la misma vista de creación
            TempData["SuccessMessage"] = $"¡Éxito! La solicitud #REQ-{solicitud.Id:D4} por {solicitud.MontoSolicitado:C} ha sido registrada y está en evaluación.";
            return RedirectToAction(nameof(Create));
        }
    }
}