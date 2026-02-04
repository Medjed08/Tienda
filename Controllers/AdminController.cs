using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Asgard_Store.Models;
using Asgard_Store.ViewModels;

namespace Asgard_Store.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Admin
        public async Task<IActionResult> Index()
        {
            var stats = new AdminDashboardViewModel
            {
                TotalProductos = await _context.Productos.CountAsync(),
                TotalCategorias = await _context.Categorias.CountAsync(),
                TotalPedidos = await _context.Pedidos.CountAsync(),
                TotalUsuarios = await _userManager.Users.CountAsync(),
                PedidosPendientes = await _context.Pedidos.CountAsync(p => p.Estado == "Pendiente"),
                VentasTotales = await _context.Pedidos.SumAsync(p => (decimal?)p.Total) ?? 0
            };

            return View(stats);
        }

        // ==================== PRODUCTOS ====================

        // GET: Admin/Productos
        public async Task<IActionResult> Productos()
        {
            var productos = await _context.Productos
                .Include(p => p.Categoria)
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            return View(productos);
        }

        // GET: Admin/CrearProducto
        public async Task<IActionResult> CrearProducto()
        {
            ViewBag.Categorias = await _context.Categorias.ToListAsync();
            return View();
        }

        // POST: Admin/CrearProducto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearProducto(Producto producto, IFormFile? imagenFile)
        {
            if (ModelState.IsValid)
            {
                producto.FechaCreacion = DateTime.Now;
                producto.Activo = true;

                // Procesar imagen si se subió
                if (imagenFile != null && imagenFile.Length > 0)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imagenFile.FileName)}";
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "productos", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imagenFile.CopyToAsync(stream);
                    }

                    producto.ImagenURL = $"/productos/{fileName}";
                }
                else
                {
                    // Imagen por defecto si no se sube ninguna
                    producto.ImagenURL = "🧦";
                }

                _context.Productos.Add(producto);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Producto creado exitosamente";
                return RedirectToAction(nameof(Productos));
            }

            ViewBag.Categorias = await _context.Categorias.ToListAsync();
            return View(producto);
        }

        // GET: Admin/EditarProducto/5
        public async Task<IActionResult> EditarProducto(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
                return NotFound();

            ViewBag.Categorias = await _context.Categorias.ToListAsync();
            return View(producto);
        }

        // POST: Admin/EditarProducto/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarProducto(int id, Producto producto, IFormFile? imagenFile)
        {
            if (id != producto.ProductoID)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Procesar nueva imagen si se subió
                    if (imagenFile != null && imagenFile.Length > 0)
                    {
                        // Eliminar imagen anterior si existe y no es un emoji
                        if (!string.IsNullOrEmpty(producto.ImagenURL) && producto.ImagenURL.StartsWith("/productos/"))
                        {
                            var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", producto.ImagenURL.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imagenFile.FileName)}";
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "productos", fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imagenFile.CopyToAsync(stream);
                        }

                        producto.ImagenURL = $"/productos/{fileName}";
                    }

                    _context.Update(producto);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Producto actualizado exitosamente";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductoExists(producto.ProductoID))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Productos));
            }

            ViewBag.Categorias = await _context.Categorias.ToListAsync();
            return View(producto);
        }

        // POST: Admin/EliminarProducto/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarProducto(int id)
        {
            var producto = await _context.Productos.FindAsync(id);

            if (producto != null)
            {
                _context.Productos.Remove(producto);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Producto eliminado exitosamente";
            }

            return RedirectToAction(nameof(Productos));
        }


        // ==================== CATEGORÍAS ====================

        // GET: Admin/Categorias
        public async Task<IActionResult> Categorias()
        {
            var categorias = await _context.Categorias
                .Include(c => c.Productos)
                .ToListAsync();

            return View(categorias);
        }

        // GET: Admin/CrearCategoria
        public IActionResult CrearCategoria()
        {
            return View();
        }

        // POST: Admin/CrearCategoria
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearCategoria(Categoria categoria)
        {
            if (ModelState.IsValid)
            {
                _context.Categorias.Add(categoria);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Categoría creada exitosamente";
                return RedirectToAction(nameof(Categorias));
            }

            return View(categoria);
        }

        // GET: Admin/EditarCategoria/5
        public async Task<IActionResult> EditarCategoria(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null)
                return NotFound();

            return View(categoria);
        }

        // POST: Admin/EditarCategoria/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarCategoria(int id, Categoria categoria)
        {
            if (id != categoria.CategoriaID)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(categoria);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Categoría actualizada exitosamente";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoriaExists(categoria.CategoriaID))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Categorias));
            }

            return View(categoria);
        }

        // POST: Admin/EliminarCategoria/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarCategoria(int id)
        {
            var categoria = await _context.Categorias
                .Include(c => c.Productos)
                .FirstOrDefaultAsync(c => c.CategoriaID == id);

            if (categoria != null)
            {
                if (categoria.Productos?.Any() == true)
                {
                    TempData["Error"] = "No se puede eliminar la categoría porque tiene productos asociados";
                }
                else
                {
                    _context.Categorias.Remove(categoria);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Categoría eliminada exitosamente";
                }
            }

            return RedirectToAction(nameof(Categorias));
        }

        // ==================== PEDIDOS ====================

        // GET: Admin/Pedidos
        public async Task<IActionResult> Pedidos()
        {
            var pedidos = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.DetallePedidos)
                    .ThenInclude(d => d.Producto)
                .OrderByDescending(p => p.FechaPedido)
                .ToListAsync();

            return View(pedidos);
        }

        // GET: Admin/DetallePedido/5
        public async Task<IActionResult> DetallePedido(int id)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.DetallePedidos)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(p => p.PedidoID == id);

            if (pedido == null)
                return NotFound();

            return View(pedido);
        }

        // POST: Admin/ActualizarEstadoPedido
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarEstadoPedido(int id, string estado)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido != null)
            {
                pedido.Estado = estado;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Estado del pedido actualizado";
            }

            return RedirectToAction(nameof(Pedidos));
        }

        // ==================== USUARIOS ====================

        // GET: Admin/Usuarios
        public async Task<IActionResult> Usuarios()
        {
            var usuarios = await _userManager.Users.ToListAsync();
            var usuariosConRoles = new List<UsuarioConRolViewModel>();

            foreach (var usuario in usuarios)
            {
                var roles = await _userManager.GetRolesAsync(usuario);
                usuariosConRoles.Add(new UsuarioConRolViewModel
                {
                    Usuario = usuario,
                    Roles = roles.ToList()
                });
            }

            return View(usuariosConRoles);
        }

        // ==================== COLORES ====================

        // GET: Admin/GestionarColores/5
        public async Task<IActionResult> GestionarColores(int id)
        {
            var producto = await _context.Productos
                .Include(p => p.Colores)
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.ProductoID == id);

            if (producto == null)
                return NotFound();

            return View(producto);
        }

        // POST: Admin/AgregarColor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarColor(int productoId, string nombreColor, string? codigoHex)
        {
            if (string.IsNullOrWhiteSpace(nombreColor))
            {
                TempData["Error"] = "El nombre del color es requerido";
                return RedirectToAction(nameof(GestionarColores), new { id = productoId });
            }

            var color = new ProductoColor
            {
                ProductoID = productoId,
                NombreColor = nombreColor.Trim(),
                CodigoHex = codigoHex?.Trim()
            };

            _context.ProductoColores.Add(color);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Color agregado exitosamente";
            return RedirectToAction(nameof(GestionarColores), new { id = productoId });
        }

        // POST: Admin/EliminarColor/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarColor(int id, int productoId)
        {
            var color = await _context.ProductoColores.FindAsync(id);
            if (color != null)
            {
                _context.ProductoColores.Remove(color);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Color eliminado exitosamente";
            }

            return RedirectToAction(nameof(GestionarColores), new { id = productoId });
        }

        // Métodos auxiliares
        private bool ProductoExists(int id)
        {
            return _context.Productos.Any(e => e.ProductoID == id);
        }

        private bool CategoriaExists(int id)
        {
            return _context.Categorias.Any(e => e.CategoriaID == id);
        }
    }
}