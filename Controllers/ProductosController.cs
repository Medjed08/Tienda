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
        public async Task<IActionResult> Categoria(int id, int? subcategoriaId = null)
        {
            var categoria = await _context.Categorias
                .Include(c => c.Subcategorias.Where(s => s.Activa))
                .FirstOrDefaultAsync(c => c.CategoriaID == id);

            if (categoria == null)
                return NotFound();

            // CARGAR TODOS LOS PRODUCTOS CON SUS VARIANTES
            var productosQuery = _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Subcategoria)
                .Include(p => p.Colores)
                    .ThenInclude(c => c.VariantesColeccion)  // Cargar variantes
                .Where(p => p.CategoriaID == id && p.Activo);

            if (subcategoriaId.HasValue)
            {
                productosQuery = productosQuery.Where(p => p.SubcategoriaID == subcategoriaId.Value);
            }

            // CARGAR EN MEMORIA PRIMERO para poder usar StockTotal
            var productos = await productosQuery.ToListAsync();

            // ORDENAR POR STOCK TOTAL (calculado)
            productos = productos.OrderByDescending(p => p.StockTotal).ToList();

            ViewBag.CategoriaNombre = categoria.Nombre;
            ViewBag.CategoriaID = id;
            ViewBag.Subcategorias = categoria.Subcategorias?.OrderBy(s => s.Orden).ToList() ?? new List<Subcategoria>();
            ViewBag.SubcategoriaSeleccionada = subcategoriaId;

            return View(productos);
        }

        // GET: Productos/Detalle/1
        public async Task<IActionResult> Detalle(int id)
        {
            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Colores)
                .ThenInclude(c => c.VariantesColeccion)
                .FirstOrDefaultAsync(p => p.ProductoID == id);

            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }
    }
}