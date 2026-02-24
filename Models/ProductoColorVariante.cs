using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asgard_Store.Models
{
    public class ProductoColorVariante
    {
        [Key]
        public int VarianteID { get; set; }

        [Required]
        public int ColorID { get; set; }

        [Required]
        [StringLength(100)]
        public string NombreVariante { get; set; } = string.Empty; // "Tipo 1", "Tipo 2", etc.

        [StringLength(500)]
        public string? ImagenUrl { get; set; } // "/images/variantes/negro-tipo1.jpg"

        // Stock individual de cada variante
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
        public int Stock { get; set; } = 0;

        [Required]
        public int Orden { get; set; } = 0; // Para ordenar las variantes

        // Navegación
        [ForeignKey("ColorID")]
        public ProductoColor? Color { get; set; }
    }
}