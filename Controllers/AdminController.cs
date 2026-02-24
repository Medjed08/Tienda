using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Asgard_Store.Models;
using Asgard_Store.ViewModels;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace Asgard_Store.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private const int ITEMS_POR_PAGINA = 10;

        public AdminController(
            ApplicationDbContext context, UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
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
        public async Task<IActionResult> Productos(int pagina = 1)
        {
            var totalProductos = await _context.Productos.CountAsync();
            var totalPaginas = (int)Math.Ceiling(totalProductos / (double)ITEMS_POR_PAGINA);

            // Validar que la página esté en rango
            if (pagina < 1) pagina = 1;
            if (pagina > totalPaginas && totalPaginas > 0) pagina = totalPaginas;

            var productos = await _context.Productos
                .Include(p => p.Categoria)
                .OrderBy(p => p.Nombre)
                .Skip((pagina - 1) * ITEMS_POR_PAGINA)
                .Take(ITEMS_POR_PAGINA)
                .ToListAsync();

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalProductos = totalProductos;

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
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "productos");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var nombreUnico = $"producto_{Guid.NewGuid()}{Path.GetExtension(imagenFile.FileName)}";
                    var rutaCompleta = Path.Combine(uploadsFolder, nombreUnico);

                    using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                    {
                        await imagenFile.CopyToAsync(stream);
                    }

                    producto.ImagenUrl = $"/images/productos/{nombreUnico}";
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
                        if (!string.IsNullOrEmpty(producto.ImagenUrl) && producto.ImagenUrl.StartsWith("/productos/"))
                        {
                            var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", producto.ImagenUrl.TrimStart('/'));
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

                        producto.ImagenUrl = $"/productos/{fileName}";
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

        // GESTIÓN DE SUBCATEGORÍAS

        [HttpGet]
        public async Task<IActionResult> GestionarSubcategorias(int id)
        {
            var categoria = await _context.Categorias
                .Include(c => c.Subcategorias)
                    .ThenInclude(s => s.Productos)
                .FirstOrDefaultAsync(c => c.CategoriaID == id);

            if (categoria == null)
            {
                TempData["Error"] = "Categoría no encontrada";
                return RedirectToAction(nameof(Categorias));
            }

            return View(categoria);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarSubcategoria(int categoriaId, string nombre, string? descripcion, int orden = 0)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                TempData["Error"] = "El nombre es requerido";
                return RedirectToAction(nameof(GestionarSubcategorias), new { id = categoriaId });
            }

            var subcategoria = new Subcategoria
            {
                CategoriaID = categoriaId,
                Nombre = nombre.Trim(),
                Descripcion = descripcion?.Trim(),
                Orden = orden,
                Activa = true
            };

            _context.Subcategorias.Add(subcategoria);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Subcategoría '{nombre}' creada exitosamente";
            return RedirectToAction(nameof(GestionarSubcategorias), new { id = categoriaId });
        }

        [HttpGet]
        public async Task<IActionResult> EditarSubcategoria(int id)
        {
            var subcategoria = await _context.Subcategorias
                .Include(s => s.Categoria)
                .FirstOrDefaultAsync(s => s.SubcategoriaID == id);

            if (subcategoria == null)
            {
                TempData["Error"] = "Subcategoría no encontrada";
                return RedirectToAction(nameof(Categorias));
            }

            return View(subcategoria);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarSubcategoria(Subcategoria subcategoria)
        {
            if (!ModelState.IsValid)
            {
                return View(subcategoria);
            }

            try
            {
                _context.Update(subcategoria);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Subcategoría actualizada exitosamente";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SubcategoriaExists(subcategoria.SubcategoriaID))
                {
                    return NotFound();
                }
                throw;
            }

            return RedirectToAction(nameof(GestionarSubcategorias), new { id = subcategoria.CategoriaID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarSubcategoria(int id)
        {
            var subcategoria = await _context.Subcategorias.FindAsync(id);

            if (subcategoria == null)
            {
                TempData["Error"] = "Subcategoría no encontrada";
                return RedirectToAction(nameof(Categorias));
            }

            var categoriaId = subcategoria.CategoriaID;

            _context.Subcategorias.Remove(subcategoria);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Subcategoría eliminada. Los productos ahora no tienen subcategoría.";
            return RedirectToAction(nameof(GestionarSubcategorias), new { id = categoriaId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DesactivarSubcategoria(int id)
        {
            var subcategoria = await _context.Subcategorias.FindAsync(id);

            if (subcategoria != null)
            {
                subcategoria.Activa = false;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Subcategoría desactivada";
            }

            return RedirectToAction(nameof(GestionarSubcategorias), new { id = subcategoria?.CategoriaID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivarSubcategoria(int id)
        {
            var subcategoria = await _context.Subcategorias.FindAsync(id);

            if (subcategoria != null)
            {
                subcategoria.Activa = true;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Subcategoría activada";
            }

            return RedirectToAction(nameof(GestionarSubcategorias), new { id = subcategoria?.CategoriaID });
        }

        // Método helper
        private bool SubcategoriaExists(int id)
        {
            return _context.Subcategorias.Any(e => e.SubcategoriaID == id);
        }

        // ==================== PEDIDOS ====================

        // GET: Admin/Pedidos
        public async Task<IActionResult> Pedidos(int pagina = 1)
        {
            var totalPedidos = await _context.Pedidos.CountAsync();
            var totalPaginas = (int)Math.Ceiling(totalPedidos / (double)ITEMS_POR_PAGINA);

            // Validar que la página esté en rango
            if (pagina < 1) pagina = 1;
            if (pagina > totalPaginas && totalPaginas > 0) pagina = totalPaginas;

            var pedidos = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.DetallePedidos)
                    .ThenInclude(d => d.Producto)
                .OrderByDescending(p => p.FechaPedido)
                .Skip((pagina - 1) * ITEMS_POR_PAGINA)
                .Take(ITEMS_POR_PAGINA)
                .ToListAsync();

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalPedidos = totalPedidos;

            return View(pedidos);
        }

        // GET: Admin/DetallePedido/5
        public async Task<IActionResult> DetallePedido(int id)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.DetallePedidos)
                    .ThenInclude(d => d.Producto)
                    .ThenInclude(p => p.Categoria)
                    .ThenInclude(p => p.Subcategorias)
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
        public async Task<IActionResult> Usuarios(int pagina = 1)
        {
            var totalUsuarios = await _userManager.Users.CountAsync();
            var totalPaginas = (int)Math.Ceiling(totalUsuarios / (double)ITEMS_POR_PAGINA);

            // Validar que la página esté en rango
            if (pagina < 1) pagina = 1;
            if (pagina > totalPaginas && totalPaginas > 0) pagina = totalPaginas;

            var usuarios = await _userManager.Users
                .OrderBy(u => u.UserName)
                .Skip((pagina - 1) * ITEMS_POR_PAGINA)
                .Take(ITEMS_POR_PAGINA)
                .ToListAsync();

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

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalUsuarios = totalUsuarios;

            return View(usuariosConRoles);
        }

        // ==================== COLORES ====================

        // GET: Admin/GestionarColores/5
        [HttpGet]
        public async Task<IActionResult> GestionarColores(int id)
        {
            var producto = await _context.Productos
                .Include(p => p.Colores)
                    .ThenInclude(c => c.VariantesColeccion)
                .FirstOrDefaultAsync(p => p.ProductoID == id);

            if (producto == null)
            {
                TempData["Error"] = "Producto no encontrado";
                return RedirectToAction(nameof(Productos));
            }

            return View(producto);
        }

        // POST: Admin/AgregarColor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarColor(
    int productoId,
    string nombreColor,
    string? codigoHex,
    string? variantes,
    IFormFile? imagenFile)
        {
            if (string.IsNullOrWhiteSpace(nombreColor))
            {
                TempData["Error"] = "El nombre del color es requerido";
                return RedirectToAction(nameof(GestionarColores), new { id = productoId });
            }

            // Manejar la subida de imagen
            string? imagenUrl = null;
            if (imagenFile != null && imagenFile.Length > 0)
            {
                try
                {
                    // Validar tipo de archivo
                    var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var extension = Path.GetExtension(imagenFile.FileName).ToLowerInvariant();

                    if (!extensionesPermitidas.Contains(extension))
                    {
                        TempData["Error"] = "Solo se permiten imágenes (JPG, PNG, GIF, WEBP)";
                        return RedirectToAction(nameof(GestionarColores), new { id = productoId });
                    }

                    // Validar tamaño (máximo 5MB)
                    if (imagenFile.Length > 5 * 1024 * 1024)
                    {
                        TempData["Error"] = "La imagen no puede superar los 5MB";
                        return RedirectToAction(nameof(GestionarColores), new { id = productoId });
                    }

                    // Crear directorio si no existe
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "colores");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Generar nombre único para la imagen
                    var nombreUnico = $"{productoId}_{nombreColor.Replace(" ", "_")}_{Guid.NewGuid()}{extension}";
                    var rutaCompleta = Path.Combine(uploadsFolder, nombreUnico);

                    // Guardar la imagen
                    using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                    {
                        await imagenFile.CopyToAsync(stream);
                    }

                    // Guardar la ruta relativa
                    imagenUrl = $"/images/colores/{nombreUnico}";
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error al subir la imagen: {ex.Message}";
                    return RedirectToAction(nameof(GestionarColores), new { id = productoId });
                }
            }

            // Crear el color
            var color = new ProductoColor
            {
                ProductoID = productoId,
                NombreColor = nombreColor.Trim(),
                CodigoHex = codigoHex?.Trim(),
                Variantes = variantes,
                ImagenUrl = imagenUrl
            };

            _context.ProductoColores.Add(color);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(imagenUrl))
            {
                TempData["Success"] = $"Color '{nombreColor}' agregado con imagen";
            }
            else if (color.TieneVariantes)
            {
                TempData["Success"] = $"Color '{nombreColor}' agregado con {color.ListaVariantes.Count} variantes";
            }
            else
            {
                TempData["Success"] = $"Color '{nombreColor}' agregado exitosamente";
            }

            return RedirectToAction(nameof(GestionarColores), new { id = productoId });
        }

        // EditarColor GET
        [HttpGet]
        public async Task<IActionResult> EditarColor(int id)
        {
            var color = await _context.ProductoColores
                .Include(c => c.Producto)
                .FirstOrDefaultAsync(c => c.ColorID == id);

            if (color == null)
            {
                TempData["Error"] = "Color no encontrado";
                return RedirectToAction(nameof(Productos));
            }

            return View(color);
        }

        // EditarColor POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarColor(
            int colorId,
            string nombreColor,
            string? codigoHex,
            string? variantes,
            IFormFile? imagenFile,
            bool eliminarImagen = false)
        {
            var color = await _context.ProductoColores.FindAsync(colorId);

            if (color == null)
            {
                TempData["Error"] = "Color no encontrado";
                return RedirectToAction(nameof(Productos));
            }

            // Actualizar datos básicos
            color.NombreColor = nombreColor.Trim();
            color.CodigoHex = codigoHex?.Trim();
            color.Variantes = variantes;

            // Manejar eliminación de imagen
            if (eliminarImagen && !string.IsNullOrEmpty(color.ImagenUrl))
            {
                try
                {
                    var rutaCompleta = Path.Combine(_webHostEnvironment.WebRootPath, color.ImagenUrl.TrimStart('/'));
                    if (System.IO.File.Exists(rutaCompleta))
                    {
                        System.IO.File.Delete(rutaCompleta);
                    }
                    color.ImagenUrl = null;
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error al eliminar la imagen: {ex.Message}";
                }
            }

            // Manejar nueva imagen
            if (imagenFile != null && imagenFile.Length > 0)
            {
                try
                {
                    // Eliminar imagen anterior si existe
                    if (!string.IsNullOrEmpty(color.ImagenUrl))
                    {
                        var rutaAnterior = Path.Combine(_webHostEnvironment.WebRootPath, color.ImagenUrl.TrimStart('/'));
                        if (System.IO.File.Exists(rutaAnterior))
                        {
                            System.IO.File.Delete(rutaAnterior);
                        }
                    }

                    // Subir nueva imagen
                    var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var extension = Path.GetExtension(imagenFile.FileName).ToLowerInvariant();

                    if (!extensionesPermitidas.Contains(extension))
                    {
                        TempData["Error"] = "Solo se permiten imágenes (JPG, PNG, GIF, WEBP)";
                        return RedirectToAction(nameof(GestionarColores), new { id = color.ProductoID });
                    }

                    if (imagenFile.Length > 5 * 1024 * 1024)
                    {
                        TempData["Error"] = "La imagen no puede superar los 5MB";
                        return RedirectToAction(nameof(GestionarColores), new { id = color.ProductoID });
                    }

                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "colores");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var nombreUnico = $"{color.ProductoID}_{nombreColor.Replace(" ", "_")}_{Guid.NewGuid()}{extension}";
                    var rutaCompleta = Path.Combine(uploadsFolder, nombreUnico);

                    using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                    {
                        await imagenFile.CopyToAsync(stream);
                    }

                    color.ImagenUrl = $"/images/colores/{nombreUnico}";
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error al subir la imagen: {ex.Message}";
                    return RedirectToAction(nameof(GestionarColores), new { id = color.ProductoID });
                }
            }

            _context.ProductoColores.Update(color);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Color '{nombreColor}' actualizado exitosamente";
            return RedirectToAction(nameof(GestionarColores), new { id = color.ProductoID });
        }

        // POST: Admin/EliminarColor/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarColor(int id)
        {
            var color = await _context.ProductoColores
                .Include(c => c.VariantesColeccion) 
                .FirstOrDefaultAsync(c => c.ColorID == id);

            if (color == null)
            {
                TempData["Error"] = "Color no encontrado";
                return RedirectToAction(nameof(Productos));
            }

            var productoId = color.ProductoID;

            // Eliminar imágenes de todas las variantes
            if (color.VariantesColeccion != null)
            {
                foreach (var variante in color.VariantesColeccion)
                {
                    if (!string.IsNullOrEmpty(variante.ImagenUrl))
                    {
                        try
                        {
                            var rutaCompleta = Path.Combine(_webHostEnvironment.WebRootPath, variante.ImagenUrl.TrimStart('/'));
                            if (System.IO.File.Exists(rutaCompleta))
                            {
                                System.IO.File.Delete(rutaCompleta);
                            }
                        }
                        catch { }
                    }
                }
            }

            _context.ProductoColores.Remove(color);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Color y sus variantes eliminados exitosamente";
            return RedirectToAction(nameof(GestionarColores), new { id = productoId });
        }


        // MÉTODOS HELPER

        private bool ProductoExists(int id)
        {
            return _context.Productos.Any(e => e.ProductoID == id);
        }

        private bool CategoriaExists(int id)
        {
            return _context.Categorias.Any(e => e.CategoriaID == id);
        }

        private bool ColorExists(int id)
        {
            return _context.ProductoColores.Any(e => e.ColorID == id);
        }

        private bool PedidoExists(int id)
        {
            return _context.Pedidos.Any(e => e.PedidoID == id);
        }

        // GESTIÓN DE COLORES BASE Y VARIANTES

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarColorBase(int productoId, string nombreColor, string? codigoHex)
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

            TempData["Success"] = $"Color base '{nombreColor}' creado. Ahora puedes agregar variantes.";
            return RedirectToAction(nameof(GestionarColores), new { id = productoId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarVariante(int colorId, string nombreVariante, int stock, IFormFile? imagenFile)
        {
            if (string.IsNullOrWhiteSpace(nombreVariante))
            {
                TempData["Error"] = "El nombre de la variante es requerido";
                var colorTemp = await _context.ProductoColores.FindAsync(colorId);
                return RedirectToAction(nameof(GestionarColores), new { id = colorTemp?.ProductoID });
            }

            var color = await _context.ProductoColores
                .Include(c => c.VariantesColeccion)
                .FirstOrDefaultAsync(c => c.ColorID == colorId);

            if (color == null)
            {
                TempData["Error"] = "Color no encontrado";
                return RedirectToAction(nameof(Productos));
            }

            var maxOrden = (color.VariantesColeccion != null && color.VariantesColeccion.Any())
            ? color.VariantesColeccion.Max(v => v.Orden)
            : -1;

            var variante = new ProductoColorVariante
            {
                ColorID = colorId,
                NombreVariante = nombreVariante.Trim(),
                Stock = stock,
                Orden = maxOrden + 1
            };

            if (imagenFile != null && imagenFile.Length > 0)
            {
                variante.ImagenUrl = await SubirImagenVarianteAsync(imagenFile, color.ProductoID, color.NombreColor, nombreVariante);
            }

            _context.ProductoColorVariantes.Add(variante);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Variante '{nombreVariante}' agregada exitosamente";
            return RedirectToAction(nameof(GestionarColores), new { id = color.ProductoID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarStockVariante(int varianteId, int nuevoStock)
        {
            if (nuevoStock < 0)
            {
                TempData["Error"] = "El stock no puede ser negativo";
                return RedirectToAction(nameof(Productos));
            }

            var variante = await _context.ProductoColorVariantes
                .Include(v => v.Color)
                .FirstOrDefaultAsync(v => v.VarianteID == varianteId);

            if (variante == null)
            {
                TempData["Error"] = "Variante no encontrada";
                return RedirectToAction(nameof(Productos));
            }

            int stockAnterior = variante.Stock;
            variante.Stock = nuevoStock;

            _context.ProductoColorVariantes.Update(variante);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Stock actualizado: {stockAnterior} → {nuevoStock} unidades";
            return RedirectToAction(nameof(GestionarColores), new { id = variante.Color.ProductoID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubirImagenVariante(int varianteId, IFormFile? imagenFile)
        {
            var variante = await _context.ProductoColorVariantes
                .Include(v => v.Color)
                .FirstOrDefaultAsync(v => v.VarianteID == varianteId);

            if (variante == null)
            {
                TempData["Error"] = "Variante no encontrada";
                return RedirectToAction(nameof(Productos));
            }

            if (imagenFile == null || imagenFile.Length == 0)
            {
                TempData["Error"] = "No se seleccionó ninguna imagen";
                return RedirectToAction(nameof(GestionarColores), new { id = variante.Color.ProductoID });
            }

            if (!string.IsNullOrEmpty(variante.ImagenUrl))
            {
                try
                {
                    var rutaAnterior = Path.Combine(_webHostEnvironment.WebRootPath, variante.ImagenUrl.TrimStart('/'));
                    if (System.IO.File.Exists(rutaAnterior))
                    {
                        System.IO.File.Delete(rutaAnterior);
                    }
                }
                catch { }
            }

            variante.ImagenUrl = await SubirImagenVarianteAsync(imagenFile, variante.Color.ProductoID, variante.Color.NombreColor, variante.NombreVariante);

            if (variante.ImagenUrl != null)
            {
                _context.ProductoColorVariantes.Update(variante);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Imagen actualizada exitosamente";
            }
            else
            {
                TempData["Error"] = "Error al subir la imagen";
            }

            return RedirectToAction(nameof(GestionarColores), new { id = variante.Color.ProductoID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarImagenVariante(int id)
        {
            var variante = await _context.ProductoColorVariantes
                .Include(v => v.Color)
                .FirstOrDefaultAsync(v => v.VarianteID == id);

            if (variante == null)
            {
                TempData["Error"] = "Variante no encontrada";
                return RedirectToAction(nameof(Productos));
            }

            if (!string.IsNullOrEmpty(variante.ImagenUrl))
            {
                try
                {
                    var rutaCompleta = Path.Combine(_webHostEnvironment.WebRootPath, variante.ImagenUrl.TrimStart('/'));
                    if (System.IO.File.Exists(rutaCompleta))
                    {
                        System.IO.File.Delete(rutaCompleta);
                    }
                }
                catch { }

                variante.ImagenUrl = null;
                _context.ProductoColorVariantes.Update(variante);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Imagen eliminada exitosamente";
            }

            return RedirectToAction(nameof(GestionarColores), new { id = variante.Color.ProductoID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarVariante(int id)
        {
            var variante = await _context.ProductoColorVariantes
                .Include(v => v.Color)
                .FirstOrDefaultAsync(v => v.VarianteID == id);

            if (variante == null)
            {
                TempData["Error"] = "Variante no encontrada";
                return RedirectToAction(nameof(Productos));
            }

            var productoId = variante.Color.ProductoID;

            if (!string.IsNullOrEmpty(variante.ImagenUrl))
            {
                try
                {
                    var rutaCompleta = Path.Combine(_webHostEnvironment.WebRootPath, variante.ImagenUrl.TrimStart('/'));
                    if (System.IO.File.Exists(rutaCompleta))
                    {
                        System.IO.File.Delete(rutaCompleta);
                    }
                }
                catch { }
            }

            _context.ProductoColorVariantes.Remove(variante);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Variante eliminada exitosamente";
            return RedirectToAction(nameof(GestionarColores), new { id = productoId });
        }

        private async Task<string?> SubirImagenVarianteAsync(IFormFile imagenFile, int productoId, string nombreColor, string nombreVariante)
        {
            try
            {
                var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var extension = Path.GetExtension(imagenFile.FileName).ToLowerInvariant();

                if (!extensionesPermitidas.Contains(extension))
                    return null;

                if (imagenFile.Length > 5 * 1024 * 1024)
                    return null;

                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "variantes");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var nombreSeguro = $"{productoId}_{nombreColor.Replace(" ", "_")}_{nombreVariante.Replace(" ", "_")}_{Guid.NewGuid()}{extension}";
                var rutaCompleta = Path.Combine(uploadsFolder, nombreSeguro);

                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    await imagenFile.CopyToAsync(stream);
                }

                return $"/images/variantes/{nombreSeguro}";
            }
            catch
            {
                return null;
            }
        }

        // OBTENER SUBCATEGORÍAS

        [HttpGet]
        public async Task<IActionResult> ObtenerSubcategorias(int categoriaId)
        {
            var subcategorias = await _context.Subcategorias
                .Where(s => s.CategoriaID == categoriaId && s.Activa)
                .OrderBy(s => s.Orden)
                .ThenBy(s => s.Nombre)
                .Select(s => new
                {
                    subcategoriaID = s.SubcategoriaID,
                    nombre = s.Nombre
                })
                .ToListAsync();

            return Json(subcategorias);
        }
    }
}


