namespace Asgard_Store.ViewModels
{
    public class CarritoItemViewModel
    {
        public int ProductoId { get; set; }
        public string Nombre { get; set; }
        public decimal Precio { get; set; }
        public int Cantidad { get; set; }
        public string? Color { get; set; }
        public string? Talla { get; set; }
    }
}