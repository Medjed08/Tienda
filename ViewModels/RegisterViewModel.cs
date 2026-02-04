using System.ComponentModel.DataAnnotations;

namespace Asgard_Store.ViewModels
{
    public class RegisterViewModel
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

        [Phone]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [StringLength(255)]
        [Display(Name = "Dirección")]
        public string? Direccion { get; set; }

        [StringLength(100)]
        [Display(Name = "Departamento")]
        public string? Departamento { get; set; }

        [StringLength(10)]
        [Display(Name = "Código Postal")]
        public string? CodigoPostal { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Contraseña")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; }
    }
}