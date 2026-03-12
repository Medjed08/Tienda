using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Asgard_Store.Models;
using Asgard_Store.Services;
using System.Text.Json;

namespace Asgard_Store.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PagoService _pagoService;
        private readonly IConfiguration _configuration;

        public CheckoutController(
            ApplicationDbContext context,
            PagoService pagoService,
            IConfiguration configuration)
        {
            _context = context;
            _pagoService = pagoService;
            _configuration = configuration;
        }

        // GET: Checkout
        public IActionResult Index()
        {
            // Pasar Public Key al frontend
            ViewBag.MercadoPagoPublicKey = _configuration["MercadoPago:PublicKey"];
            return View();
        }

        // POST: Procesar pedido y pago
        [HttpPost]
        public async Task<IActionResult> ProcesarPedido([FromBody] PedidoConPagoRequest request)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, mensaje = "Datos inválidos" });
            }

            try
            {
                // 1. DESERIALIZAR CARRITO
                var itemsCarrito = request.Items;

                if (itemsCarrito == null || !itemsCarrito.Any())
                {
                    return Json(new { success = false, mensaje = "El carrito está vacío" });
                }

                // 2. CREAR O BUSCAR CLIENTE
                var cliente = await _context.Clientes
                    .FirstOrDefaultAsync(c => c.Email == request.Email);

                if (cliente == null)
                {
                    cliente = new Cliente
                    {
                        Nombre = request.Nombre,
                        Apellido = request.Apellido,
                        Email = request.Email,
                        Telefono = request.Telefono,
                        Direccion = request.DireccionEnvio,
                        FechaRegistro = DateTime.Now
                    };

                    _context.Clientes.Add(cliente);
                    await _context.SaveChangesAsync();
                }

                // 3. CREAR PEDIDO (estado pendiente)
                var pedido = new Pedido
                {
                    ClienteID = cliente.ClienteID,
                    FechaPedido = DateTime.Now,
                    Estado = "Procesando Pago",
                    DireccionEnvio = request.DireccionEnvio,
                    Departamento = request.Departamento,
                    CodigoPostal = request.CodigoPostal,
                    Total = 0
                };

                _context.Pedidos.Add(pedido);
                await _context.SaveChangesAsync();

                decimal total = 0;

                // 4. PROCESAR ITEMS Y VALIDAR STOCK
                foreach (var item in itemsCarrito)
                {
                    var producto = await _context.Productos
                        .Include(p => p.Colores)
                            .ThenInclude(c => c.VariantesColeccion)
                        .FirstOrDefaultAsync(p => p.ProductoID == item.ProductoId);

                    if (producto == null) continue;

                    // Parsear color y variante
                    ProductoColorVariante? varianteSeleccionada = null;

                    if (!string.IsNullOrEmpty(item.Color))
                    {
                        var partes = item.Color.Split(new[] { " - " }, StringSplitOptions.None);
                        string nombreColor = partes[0].Trim();
                        string? nombreVariante = partes.Length > 1 ? partes[1].Trim() : null;

                        var color = producto.Colores?
                            .FirstOrDefault(c => c.NombreColor.Equals(nombreColor, StringComparison.OrdinalIgnoreCase));

                        if (color != null && !string.IsNullOrEmpty(nombreVariante))
                        {
                            varianteSeleccionada = color.VariantesColeccion?
                                .FirstOrDefault(v => v.NombreVariante.Equals(nombreVariante, StringComparison.OrdinalIgnoreCase));
                        }
                    }

                    // Validar y reservar stock
                    bool stockDisponible = false;

                    if (varianteSeleccionada != null)
                    {
                        if (varianteSeleccionada.Stock >= item.Cantidad)
                        {
                            varianteSeleccionada.Stock -= item.Cantidad;
                            _context.ProductoColorVariantes.Update(varianteSeleccionada);
                            stockDisponible = true;
                        }
                    }
                    else
                    {
                        int stockProducto = producto.Stock;

                        if (stockProducto >= item.Cantidad)
                        {
                            producto.Stock = stockProducto - item.Cantidad;
                            _context.Productos.Update(producto);
                            stockDisponible = true;
                        }
                    }

                    if (!stockDisponible)
                    {
                        _context.Pedidos.Remove(pedido);
                        await _context.SaveChangesAsync();

                        return Json(new
                        {
                            success = false,
                            mensaje = $"No hay stock suficiente de {producto.Nombre}"
                        });
                    }

                    // Agregar detalle del pedido
                    var detalle = new DetallePedido
                    {
                        PedidoID = pedido.PedidoID,
                        ProductoID = item.ProductoId,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = item.Precio,
                        Color = item.Color,
                        Talla = item.Talla ?? "Única",
                        NombreProducto = producto.Nombre
                    };

                    _context.DetallePedidos.Add(detalle);
                    total += item.Precio * item.Cantidad;
                }

                pedido.Total = total;
                await _context.SaveChangesAsync();

                // 5. PROCESAR PAGO CON MERCADO PAGO
                var datosPago = new DatosPagoRequest
                {
                    TokenTarjeta = request.TokenTarjeta,
                    Monto = total,
                    Descripcion = $"Pedido #{pedido.PedidoID} - Asgard Store",
                    Cuotas = request.Cuotas,
                    MetodoPago = request.MetodoPago,
                    EmailCliente = cliente.Email,
                    NombreCliente = cliente.Nombre,
                    ApellidoCliente = cliente.Apellido,
                    DocumentoCliente = request.Documento,
                    PedidoId = pedido.PedidoID
                };

                var resultado = await _pagoService.ProcesarPagoTarjeta(datosPago);

                // 6. ACTUALIZAR PEDIDO SEGÚN RESULTADO
                if (resultado.Exitoso)
                {
                    pedido.Estado = "Confirmado";
                    pedido.FechaPago = resultado.FechaPago;
                    pedido.PagoMP = resultado.PagoId;
                    await _context.SaveChangesAsync();

                    return Json(new
                    {
                        success = true,
                        pedidoId = pedido.PedidoID,
                        mensaje = "Pago procesado exitosamente"
                    });
                }
                else
                {
                    pedido.Estado = "Pago Rechazado";
                    await _context.SaveChangesAsync();

                    // Opcional: liberar stock si el pago falla
                    // TODO: Implementar liberación de stock

                    return Json(new
                    {
                        success = false,
                        mensaje = resultado.MensajeError ?? "Pago rechazado"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    mensaje = $"Error: {ex.Message}"
                });
            }
        }

        // GET: Confirmación
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

            return View(pedido);
        }

        // GET: Error
        public IActionResult Error(string mensaje = null)
        {
            ViewBag.MensajeError = mensaje ?? "Hubo un problema al procesar tu pago";
            return View();
        }
    }

    // ===== MODELOS =====

    public class PedidoConPagoRequest
    {
        // Datos del cliente
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string Documento { get; set; }

        // Dirección
        public string DireccionEnvio { get; set; } 
        public string Departamento { get; set; }
        public string CodigoPostal { get; set; }

        // Items del carrito
        public List<ItemCarrito> Items { get; set; }

        // Datos del pago (token de Mercado Pago)
        public string TokenTarjeta { get; set; }
        public int Cuotas { get; set; } = 1;
        public string MetodoPago { get; set; }
    }

    public class ItemCarrito
    {
        public int ProductoId { get; set; }
        public string Nombre { get; set; }
        public decimal Precio { get; set; }
        public int Cantidad { get; set; }
        public string? Color { get; set; }
        public string? Talla { get; set; }
    }
}