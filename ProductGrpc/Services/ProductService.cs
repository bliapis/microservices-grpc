using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using ProductGrpc.Data;
using ProductGrpc.Protos;
using System;
using System.Threading.Tasks;

namespace ProductGrpc.Services
{
    public class ProductService : ProductProtoService.ProductProtoServiceBase
    {
        private readonly ProductsContext _context;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            ProductsContext context,
            ILogger<ProductService> logger)
        {
            _context = context;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override Task<Empty> Test(Empty request, ServerCallContext context)
        {
            return base.Test(request, context);
        }

        public override async Task<ProductModel> GetProduct(GetProductRequest request, ServerCallContext context)
        {
            var product = await _context.Product.FindAsync(request.ProductId);

            if (product is null)
            {
                // throw rpc ex
            }

            var productModel = new ProductModel
            {
                ProductId = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Status = (ProductStatus)product.Status,
                CreatedTime = Timestamp.FromDateTime(product.CreatedTime)
            };

            return productModel;
        }
    }
}