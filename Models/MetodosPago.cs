using System;
using System.Collections.Generic;

namespace Asgard_Store.Models;

public partial class MetodosPago
{
    public int MetodoPagoId { get; set; }

    public int PedidoId { get; set; }

    public string TipoPago { get; set; } = null!;

    public string? EstadoPago { get; set; }

    public DateTime? FechaPago { get; set; }

    public virtual Pedido Pedido { get; set; } = null!;
}
