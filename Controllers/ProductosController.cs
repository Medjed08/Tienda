using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Asgard_Store.Models;

namespace Asgard_Store.Controllers
{
    public class ProductosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Productos/Categoria/1
        public async Task<IActionResult> Categoria(int id)
        {
            var categoria = await _context.Categorias
                .Include(c => c.Productos)
                    .ThenInclude(p => p.Colores)  // ✅ AGREGAR ESTA LÍNEA
                .FirstOrDefaultAsync(c => c.CategoriaID == id);

            if (categoria == null)
            {
                return NotFound();
            }

            ViewBag.CategoriaNombre = categoria.Nombre;
            ViewBag.CategoriaDescripcion = categoria.Descripcion;
            ViewBag.CategoriaIcono = categoria.Icono;

            var productosActivos = categoria.Productos?
                .Where(p => p.Activo && p.Stock > 0)
                .ToList() ?? new List<Producto>();

            return View(productosActivos);
        }

        // GET: Productos/Detalle/1
        public async Task<IActionResult> Detalle(int id)
        {
            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Colores)  // ✅ AGREGAR ESTA LÍNEA
                .FirstOrDefaultAsync(p => p.ProductoID == id);

            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }
    }
}