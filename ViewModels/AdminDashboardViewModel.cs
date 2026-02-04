namespace Asgard_Store.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalProductos { get; set; }
        public int TotalCategorias { get; set; }
        public int TotalPedidos { get; set; }
        public int TotalUsuarios { get; set; }
        public int PedidosPendientes { get; set; }
        public decimal VentasTotales { get; set; }
    }
}