using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asgard_Store.Models
{
    public class DetallePedido
    {
        [Key]
        public int DetalleID { get; set; }

        [Required]
        public int PedidoID { get; set; }

        public int? ProductoID { get; set; }

        [Required]
        public int Cantidad { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioUnitario { get; set; }

        [StringLength(50)]
        public string? Color { get; set; }

        [StringLength(10)]
        public string? Talla { get; set; }

        [StringLength(100)]
        public string? NombreProducto { get; set; }

        [NotMapped]
        public decimal Subtotal => Cantidad * PrecioUnitario;

        [ForeignKey("PedidoID")]
        public Pedido? Pedido { get; set; }

        [ForeignKey("ProductoID")]
        public Producto? Producto { get; set; }
    }
}