using Asgard_Store.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Pedido
{
    [Key]
    public int PedidoID { get; set; }

    [Required]
    public int ClienteID { get; set; }

    [Required]
    public DateTime FechaPedido { get; set; }

    [Required]
    [StringLength(50)]
    public string Estado { get; set; } = "Pendiente de Pago";

    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    public decimal Total { get; set; }

    [StringLength(500)]
    public string? DireccionEnvio { get; set; }

    [StringLength(100)]
    public string? Departamento { get; set; }

    [StringLength(20)]
    public string? CodigoPostal { get; set; }

    [StringLength(100)]
    public string? PreferenciaMP { get; set; }

    public DateTime? FechaPago { get; set; }

    [StringLength(100)]
    public string? PagoMP { get; set; }

    // Relaciones
    [ForeignKey("ClienteID")]
    public virtual Cliente? Cliente { get; set; }

    public virtual ICollection<DetallePedido>? DetallePedidos { get; set; }
}