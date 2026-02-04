using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asgard_Store.Models
{
    public class Pedido
    {
        [Key]
        public int PedidoID { get; set; }

        [Required]
        public int ClienteID { get; set; }

        public string? ApplicationUserId { get; set; }

        public DateTime FechaPedido { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Total { get; set; }

        [StringLength(50)]
        public string Estado { get; set; } = "Pendiente";

        [StringLength(255)]
        public string? DireccionEnvio { get; set; }

        [ForeignKey("ClienteID")]
        public Cliente? Cliente { get; set; }

        [ForeignKey("ApplicationUserId")]
        public ApplicationUser? Usuario { get; set; }

        public ICollection<DetallePedido>? DetallePedidos { get; set; }
    }
}