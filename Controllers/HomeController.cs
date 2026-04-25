using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RiskPortal.Data;
using RiskPortal.Models;
using Plataforma_de_Créditos___Gestión_de_Solicitudes_y_Evaluación.Models;

namespace Plataforma_de_Créditos___Gestión_de_Solicitudes_y_Evaluación.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.TotalMes = await _context.Solicitudes.CountAsync();
        ViewBag.Pendientes = await _context.Solicitudes.CountAsync(s => s.Estado == EstadoSolicitud.Pendiente);
        ViewBag.Aprobadas = await _context.Solicitudes.CountAsync(s => s.Estado == EstadoSolicitud.Aprobado);
        ViewBag.Rechazadas = await _context.Solicitudes.CountAsync(s => s.Estado == EstadoSolicitud.Rechazado);

        var solicitudesRecientes = await _context.Solicitudes
            .Include(s => s.Cliente)
            .OrderByDescending(s => s.FechaSolicitud)
            .Take(10)
            .ToListAsync();

        return View(solicitudesRecientes);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
