using System.ComponentModel.DataAnnotations;

namespace Asgard_Store.Models
{
    public class Categoria
    {
        [Key]
        public int CategoriaID { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [StringLength(255)]
        public string? Descripcion { get; set; }

        [StringLength(10)]
        public string? Icono { get; set; }

        public ICollection<Producto>? Productos { get; set; }
    }
}