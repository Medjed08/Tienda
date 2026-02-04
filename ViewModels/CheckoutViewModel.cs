using System.ComponentModel.DataAnnotations;

namespace Asgard_Store.ViewModels
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es requerido")]
        [StringLength(100)]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; }

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email no válido")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "El teléfono es requerido")]
        [Phone(ErrorMessage = "Teléfono no válido")]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; }

        [Required(ErrorMessage = "La dirección es requerida")]
        [StringLength(255)]
        [Display(Name = "Dirección de Envío")]
        public string Direccion { get; set; }

        [Required(ErrorMessage = "El departamento es requerido")]
        [StringLength(100)]
        [Display(Name = "Departamento")]
        public string Departamento { get; set; }

        [Required(ErrorMessage = "El código postal es requerido")]
        [StringLength(10)]
        [Display(Name = "Código Postal")]
        public string CodigoPostal { get; set; }

        [Display(Name = "Notas del Pedido")]
        [StringLength(500)]
        public string? Notas { get; set; }
    }
}