using ProductGrpc.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProductGrpc.Data
{
    public class ProductsContextSeed
    {
        public static void SeedAsync(ProductsContext context)
        {
            if (!context.Product.Any())
            {
                var products = new List<Product>
                {
                    new Product
                    {
                        Id = 1,
                        Name = "Prod 1",
                        Description = "Description prod 1",
                        Price = 928,
                        Status = ProductStatus.LOW,
                        CreatedTime = DateTime.UtcNow
                    },
                    new Product
                    {
                        Id = 2,
                        Name = "Prod 2",
                        Description = "Description prod 2",
                        Price = 123,
                        Status = ProductStatus.OUTSTOCK,
                        CreatedTime = DateTime.UtcNow
                    },
                    new Product
                    {
                        Id = 3,
                        Name = "Prod 3",
                        Description = "Description prod 3",
                        Price = 99,
                        Status = ProductStatus.INSTOCK,
                        CreatedTime = DateTime.UtcNow
                    },
                    new Product
                    {
                        Id = 4,
                        Name = "Prod 4",
                        Description = "Description prod 4",
                        Price = 1430,
                        Status = ProductStatus.INSTOCK,
                        CreatedTime = DateTime.UtcNow
                    }
                };

                context.Product.AddRange(products);
                context.SaveChanges();
            }
        }
    }
}