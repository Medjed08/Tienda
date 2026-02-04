using System;
using System.Collections.Generic;

namespace Asgard_Store.Models;

public partial class ProductoTalla
{
    public int TallaId { get; set; }

    public int ProductoId { get; set; }

    public string NombreTalla { get; set; } = null!;

    public virtual Producto Producto { get; set; } = null!;
}
