using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asgard_Store.Models
{
    public class Subcategoria
    {
        [Key]
        public int SubcategoriaID { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descripcion { get; set; }

        [Required]
        public int CategoriaID { get; set; }

        [Required]
        public bool Activa { get; set; } = true;

        public int Orden { get; set; } = 0; // Para ordenar subcategorías

        // Navegación
        [ForeignKey("CategoriaID")]
        public Categoria? Categoria { get; set; }

        public ICollection<Producto>? Productos { get; set; }
    }
}