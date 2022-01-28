using DiscountGrpc.Model;
using System.Collections.Generic;

namespace DiscountGrpc.Data
{
    public class DiscountContext
    {
        public static readonly List<Discount> Discount = new List<Discount>
        {
            new Discount{DiscountId = 1, Code = "CDD100", Amount = 100 },
            new Discount{DiscountId = 2, Code = "CDD200", Amount = 200 },
            new Discount{DiscountId = 3, Code = "CDD300", Amount = 300 }
        };
    }
}