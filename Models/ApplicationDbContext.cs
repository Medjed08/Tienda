using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Asgard_Store.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<ProductoColor> ProductoColores { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<DetallePedido> DetallePedidos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de Producto
            modelBuilder.Entity<Producto>(entity =>
            {
                entity.HasKey(p => p.ProductoID);
                entity.Property(p => p.Precio).HasPrecision(10, 2);
            });

            // Configuración de ProductoColor
            modelBuilder.Entity<ProductoColor>(entity =>
            {
                entity.HasKey(pc => pc.ColorID);
                entity.HasOne(pc => pc.Producto)
                    .WithMany(p => p.Colores)
                    .HasForeignKey(pc => pc.ProductoID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración de Pedido
            modelBuilder.Entity<Pedido>(entity =>
            {
                entity.HasKey(p => p.PedidoID);
                entity.Property(p => p.Total).HasPrecision(10, 2);

                entity.HasOne(p => p.Cliente)
                    .WithMany(c => c.Pedidos)
                    .HasForeignKey(p => p.ClienteID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración de DetallePedido 
            modelBuilder.Entity<DetallePedido>(entity =>
            {
                entity.HasKey(d => d.DetalleID);
                entity.Property(d => d.PrecioUnitario).HasPrecision(10, 2);

                entity.HasOne(d => d.Pedido)
                    .WithMany(p => p.DetallePedidos)
                    .HasForeignKey(d => d.PedidoID);

                entity.HasOne(d => d.Producto)
                    .WithMany()
                    .HasForeignKey(d => d.ProductoID)
                    .OnDelete(DeleteBehavior.SetNull) 
                    .IsRequired(false); 
            });
        }
    }
}