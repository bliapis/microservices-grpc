using DiscountGrpc.Data;
using DiscountGrpc.Protos;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DiscountGrpc.Services
{
    public class DiscountService : DiscountProtoService.DiscountProtoServiceBase
    {
        private readonly ILogger<DiscountService> _logger;

        public DiscountService(ILogger<DiscountService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override Task<DiscountModel> GetDiscount(GetDiscountRequest request,
            ServerCallContext context)
        {
            var discount = DiscountContext.Discount.FirstOrDefault(d => d.Code == request.DiscountCode);

            if (discount is null)
            {
                _logger.LogInformation($"Discount code {request.DiscountCode} does not exists.");

                throw new RpcException(new Status(StatusCode.NotFound,
                    $"Discount code {request.DiscountCode} does not exists."));
            }

            var discountModel = new DiscountModel()
            {
                DiscountId = discount.DiscountId,
                Code = discount.Code,
                Amount = discount.Amount
            };

            return Task.FromResult(discountModel);
        }
    }
}