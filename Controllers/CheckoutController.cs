using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Asgard_Store.Models;
using Asgard_Store.ViewModels;
using System.Text.Json;

namespace Asgard_Store.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CheckoutController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Checkout
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            var model = new CheckoutViewModel
            {
                Nombre = user.Nombre ?? "",
                Apellido = user.Apellido ?? "",
                Email = user.Email ?? "",
                Telefono = user.PhoneNumber ?? "",
                Direccion = user.Direccion ?? "",
                Departamento = user.Departamento ?? "",
                CodigoPostal = user.CodigoPostal ?? ""
            };

            return View(model);
        }

        // POST: Checkout/ProcesarPedido
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcesarPedido(CheckoutViewModel model, string carritoJson)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);

                // Deserializar el carrito desde JSON
                var itemsCarrito = JsonSerializer.Deserialize<List<CarritoItemViewModel>>(carritoJson);

                if (itemsCarrito == null || !itemsCarrito.Any())
                {
                    TempData["Error"] = "El carrito está vacío";
                    return RedirectToAction("Index", "Home");
                }

                // Buscar o crear cliente
                var cliente = await _context.Clientes
                    .FirstOrDefaultAsync(c => c.Email == user.Email);

                if (cliente == null)
                {
                    cliente = new Cliente
                    {
                        Nombre = model.Nombre,
                        Apellido = model.Apellido,
                        Email = model.Email,
                        Telefono = model.Telefono,
                        Direccion = model.Direccion,
                        Departamento = model.Departamento,
                        CodigoPostal = model.CodigoPostal,
                        FechaRegistro = DateTime.Now
                    };
                    _context.Clientes.Add(cliente);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    // Actualizar datos del cliente
                    cliente.Nombre = model.Nombre;
                    cliente.Apellido = model.Apellido;
                    cliente.Telefono = model.Telefono;
                    cliente.Direccion = model.Direccion;
                    cliente.Departamento = model.Departamento;
                    cliente.CodigoPostal = model.CodigoPostal;
                    await _context.SaveChangesAsync();
                }

                // Crear el pedido
                decimal total = 0;
                var pedido = new Pedido
                {
                    ClienteID = cliente.ClienteID,
                    FechaPedido = DateTime.Now,
                    Estado = "Pendiente",
                    DireccionEnvio = $"{model.Direccion}, {model.Departamento}, CP: {model.CodigoPostal}"
                };

                _context.Pedidos.Add(pedido);
                await _context.SaveChangesAsync();

                // Crear detalles del pedido
                foreach (var item in itemsCarrito)
                {
                    var producto = await _context.Productos.FindAsync(item.ProductoId);

                    if (producto != null && producto.Stock >= item.Cantidad)
                    {
                        var detalle = new DetallePedido
                        {
                            PedidoID = pedido.PedidoID,
                            ProductoID = item.ProductoId,
                            Cantidad = item.Cantidad,
                            PrecioUnitario = item.Precio,
                            Color = item.Color,
                            Talla = item.Talla,
                            NombreProducto = producto?.Nombre
                        };

                        _context.DetallePedidos.Add(detalle);

                        // Reducir stock
                        producto.Stock -= item.Cantidad;

                        total += item.Precio * item.Cantidad;
                    }
                }

                // Actualizar total del pedido
                pedido.Total = total;
                await _context.SaveChangesAsync();

                // Guardar el ID del pedido en TempData para mostrarlo en la confirmación
                TempData["PedidoID"] = pedido.PedidoID;
                TempData["Success"] = "¡Pedido realizado exitosamente!";

                return RedirectToAction("Confirmacion", new { id = pedido.PedidoID });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al procesar el pedido: {ex.Message}";
                return View("Index", model);
            }
        }

        // GET: Checkout/Confirmacion/5
        public async Task<IActionResult> Confirmacion(int id)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.DetallePedidos)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(p => p.PedidoID == id);

            if (pedido == null)
            {
                return NotFound();
            }

            // Verificar que el pedido pertenece al usuario actual
            var user = await _userManager.GetUserAsync(User);
            if (pedido.Cliente?.Email != user?.Email)
            {
                return Forbid();
            }

            return View(pedido);
        }

        // GET: Checkout/MisPedidos
        public async Task<IActionResult> MisPedidos()
        {
            var user = await _userManager.GetUserAsync(User);

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Email == user.Email);

            if (cliente == null)
            {
                return View(new List<Pedido>());
            }

            var pedidos = await _context.Pedidos
                .Include(p => p.DetallePedidos)
                    .ThenInclude(d => d.Producto)
                .Where(p => p.ClienteID == cliente.ClienteID)
                .OrderByDescending(p => p.FechaPedido)
                .ToListAsync();

            return View(pedidos);
        }
    }
}