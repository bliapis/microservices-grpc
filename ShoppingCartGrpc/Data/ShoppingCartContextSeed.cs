using ShoppingCartGrpc.Models;
using System.Collections.Generic;

namespace ShoppingCartGrpc.Data
{
    public class ShoppingCartContextSeed
    {
        public static void SeedAsync(ShoppingCartContext shoppingCartContext)
        {
            var shoppingCarts = new List<ShoppingCart>
            {
                new ShoppingCart
                {
                    UserName = "Sys",
                    Items = new List<ShoppingCartItem>
                    {
                        new ShoppingCartItem
                        {
                            Quantity = 2,
                            Color = "Red",
                            Price = 998,
                            ProductId = 1,
                            ProductName = "Feijuca"
                        },
                        new ShoppingCartItem
                        {
                            Quantity = 3,
                            Color = "Blue",
                            Price = 222,
                            ProductId = 2,
                            ProductName = "Mocoto"
                        }
                    }
                }
            };

            shoppingCartContext.ShoppingCart.AddRange(shoppingCarts);
            shoppingCartContext.SaveChanges();
        }
    }
}