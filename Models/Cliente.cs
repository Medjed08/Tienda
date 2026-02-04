using System.ComponentModel.DataAnnotations;

namespace Asgard_Store.Models
{
    public class Cliente
    {
        [Key]
        public int ClienteID { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Required]
        [StringLength(100)]
        public string Apellido { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Phone]
        [StringLength(20)]
        public string? Telefono { get; set; }

        [StringLength(255)]
        public string? Direccion { get; set; }

        [StringLength(100)]
        public string? Departamento { get; set; }

        [StringLength(10)]
        public string? CodigoPostal { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        public ICollection<Pedido>? Pedidos { get; set; }
    }
}