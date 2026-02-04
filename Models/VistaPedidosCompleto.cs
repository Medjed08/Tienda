using System;
using System.Collections.Generic;

namespace Asgard_Store.Models;

public partial class VistaPedidosCompleto
{
    public int PedidoId { get; set; }

    public string Cliente { get; set; } = null!;

    public string Email { get; set; } = null!;

    public DateTime? FechaPedido { get; set; }

    public decimal Total { get; set; }

    public string? Estado { get; set; }

    public string? TipoPago { get; set; }

    public string? EstadoPago { get; set; }
}
