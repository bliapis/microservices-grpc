using DiscountGrpc.Model;
using System.Collections.Generic;

namespace DiscountGrpc.Data
{
    public class DiscountContext
    {
        public static readonly List<Discount> Discount = new List<Discount>
        {
            new Discount{DiscountId = 1, Code = "COD100", Amount = 100 },
            new Discount{DiscountId = 2, Code = "COD200", Amount = 200 },
            new Discount{DiscountId = 3, Code = "COD300", Amount = 300 }
        };
    }
}