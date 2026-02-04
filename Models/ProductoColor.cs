using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asgard_Store.Models
{
    public class ProductoColor
    {
        [Key]
        public int ColorID { get; set; }

        [Required]
        public int ProductoID { get; set; }

        [Required]
        [StringLength(50)]
        public string NombreColor { get; set; }

        [StringLength(7)]
        public string? CodigoHex { get; set; }  

        [ForeignKey("ProductoID")]
        public Producto? Producto { get; set; }
    }
}