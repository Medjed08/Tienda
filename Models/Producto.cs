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
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descripcion { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Precio { get; set; }

        [Required]
        public int Stock { get; set; }

        [Required]
        public int CategoriaID { get; set; }

        // SubcategoriaID
        public int? SubcategoriaID { get; set; }

        [StringLength(500)]
        public string? ImagenUrl { get; set; }

        [Required]
        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Navegación
        [ForeignKey("CategoriaID")]
        public Categoria? Categoria { get; set; }

        // Navegación a Subcategoría
        [ForeignKey("SubcategoriaID")]
        public Subcategoria? Subcategoria { get; set; }

        public ICollection<ProductoColor>? Colores { get; set; }

        [NotMapped]
        public int StockTotal
        {
            get
            {
                if (Colores == null || !Colores.Any())
                    return Stock;

                return Colores
                    .Where(c => c.VariantesColeccion != null)
                    .SelectMany(c => c.VariantesColeccion)
                    .Sum(v => v.Stock);
            }
        }

        [NotMapped]
        public bool TieneStockDisponible => StockTotal > 0;
    }
}