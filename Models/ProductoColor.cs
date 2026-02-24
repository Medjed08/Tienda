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
        public string NombreColor { get; set; } = string.Empty; // "Negro", "Azul"

        [StringLength(7)]
        public string? CodigoHex { get; set; } // "#000000"

        // Mantener por compatibilidad pero migrar a ProductoColorVariante
        [StringLength(200)]
        [Obsolete("Usar ProductoColorVariante en su lugar")]
        public string? Variantes { get; set; }

        // Las imágenes ahora están en ProductoColorVariante
        [StringLength(500)]
        [Obsolete("Usar ProductoColorVariante.ImagenUrl en su lugar")]
        public string? ImagenUrl { get; set; }

        [ForeignKey("ProductoID")]
        public Producto? Producto { get; set; }

        // Colección de variantes (cada una con su imagen)
        public ICollection<ProductoColorVariante>? VariantesColeccion { get; set; }

        // Propiedad computada para verificar si tiene variantes
        [NotMapped]
        public bool TieneVariantes => VariantesColeccion != null && VariantesColeccion.Any();

        // MANTENER POR COMPATIBILIDAD - Para migración gradual
        [NotMapped]
        public List<string> ListaVariantes
        {
            get
            {
                if (VariantesColeccion != null && VariantesColeccion.Any())
                {
                    return VariantesColeccion.OrderBy(v => v.Orden).Select(v => v.NombreVariante).ToList();
                }

                // Fallback al sistema antiguo
                if (string.IsNullOrEmpty(Variantes))
                    return new List<string>();
                return Variantes.Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();
            }
        }
    }
}