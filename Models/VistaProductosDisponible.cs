using System;
using System.Collections.Generic;

namespace Asgard_Store.Models;

public partial class VistaProductosDisponible
{
    public int ProductoId { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public decimal Precio { get; set; }

    public int Stock { get; set; }

    public string Categoria { get; set; } = null!;

    public string? CategoriaIcono { get; set; }
}
