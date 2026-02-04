using Microsoft.AspNetCore.Mvc;
using Asgard_Store.Models;
using Microsoft.EntityFrameworkCore;

namespace Asgard_Store.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Obtener solo categorías que tienen productos activos
            var categorias = await _context.Categorias
                .Include(c => c.Productos.Where(p => p.Activo)) 
                .ToListAsync();

            return View(categorias);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}