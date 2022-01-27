using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductGrpc.Data;
using ProductGrpc.Models;
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

        public override async Task GetAllProducts(GetAllProductsRequest request,
            IServerStreamWriter<ProductModel> responseStream,
            ServerCallContext context)
        {
            var products = await _context.Product.ToListAsync();

            foreach (var product in products)
            {
                var productModel = new ProductModel
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    Status = (Protos.ProductStatus)product.Status,
                    CreatedTime = Timestamp.FromDateTime(product.CreatedTime)
                };

                await responseStream.WriteAsync(productModel);
            }
        }

        public override async Task<ProductModel> AddProduct(AddProductRequest request, 
            ServerCallContext context)
        {
            var product = new Product
            {
                Id = request.Product.ProductId,
                Name = request.Product.Name,
                Description = request.Product.Description,
                Price = request.Product.Price,
                Status = (Models.ProductStatus)request.Product.Status,
                CreatedTime = request.Product.CreatedTime.ToDateTime()
            };

            _context.Product.Add(product);

            await _context.SaveChangesAsync();

            var productModel = new ProductModel
            {
                ProductId = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Status = (Protos.ProductStatus)product.Status,
                CreatedTime = Timestamp.FromDateTime(product.CreatedTime)
            };

            return productModel;
        }
    }
}