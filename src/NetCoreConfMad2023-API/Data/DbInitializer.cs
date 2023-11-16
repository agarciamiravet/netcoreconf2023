using NetCoreConfMad2023_API.Models;

namespace NetCoreConfMad2023_API.Data
{

    public static class DbInitializer
    {
        public static void Initialize(OnlineShoppingContext context)
        {
            context.Database.EnsureCreated();

            if (context.Products.Any())
            {
                return;   // DB has been seeded
            }



            var products = new Product[]
            {
                new Product { Id = new Guid(), Name = "Producto1", Description = "Descripcion Producto 1", Price = 30 },
                new Product { Id = new Guid(), Name = "Producto2", Description = "Descripcion Producto 2", Price = 30 },
                new Product { Id = new Guid(), Name = "Producto3", Description = "Descripcion Producto 3", Price = 30 },
                new Product { Id = new Guid(), Name = "Producto4", Description = "Descripcion Producto 4", Price = 30 }
            };

            foreach (Product product in products)
            {
                context.Products.Add(product);
            }

            context.SaveChanges();
        }
    }
}
