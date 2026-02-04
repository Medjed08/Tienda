using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asgard_Store.Models
{
    public class Producto
    {
        [Key]
        public int ProductoID { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [StringLength(500)]
        public string? Descripcion { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Precio { get; set; }

        public int Stock { get; set; }

        public int? CategoriaID { get; set; }

        [StringLength(255)]
        public string? ImagenURL { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public bool Activo { get; set; } = true;

        [ForeignKey("CategoriaID")]
        public Categoria? Categoria { get; set; }

        // Relación con Colores
        public ICollection<ProductoColor>? Colores { get; set; }
    }
}