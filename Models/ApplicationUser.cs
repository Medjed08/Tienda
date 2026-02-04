using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Asgard_Store.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(100)]
        public string? Nombre { get; set; }

        [StringLength(100)]
        public string? Apellido { get; set; }

        [StringLength(255)]
        public string? Direccion { get; set; }

        [StringLength(100)]
        public string? Departamento { get; set; }

        [StringLength(10)]
        public string? CodigoPostal { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        // Relación con pedidos
        public ICollection<Pedido>? Pedidos { get; set; }
    }
}