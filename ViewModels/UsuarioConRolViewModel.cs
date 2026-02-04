using Asgard_Store.Models;

namespace Asgard_Store.ViewModels
{
    public class UsuarioConRolViewModel
    {
        public ApplicationUser Usuario { get; set; }
        public List<string> Roles { get; set; }
    }
}