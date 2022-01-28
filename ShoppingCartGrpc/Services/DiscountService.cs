using DiscountGrpc.Protos;
using System;
using System.Threading.Tasks;

namespace ShoppingCartGrpc.Services
{
    public class DiscountService
    {
        private readonly DiscountProtoService.DiscountProtoServiceClient _discountService;

        public DiscountService(DiscountProtoService.DiscountProtoServiceClient discountService)
        {
            _discountService = discountService ?? throw new ArgumentNullException(nameof(discountService));
        }

        public async Task<DiscountModel> GetDiscount(string discountCode)
        {
            var discountRequest = new GetDiscountRequest
            {
                DiscountCode = discountCode
            };

            return await _discountService.GetDiscountAsync(discountRequest);
        }
    }
}