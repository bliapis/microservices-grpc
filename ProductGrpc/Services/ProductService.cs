using AutoMapper;
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
        private readonly IMapper _mapper;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            ProductsContext context,
            IMapper mapper,
            ILogger<ProductService> logger)
        {
            _context = context;
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
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

            var productModel = _mapper.Map<ProductModel>(product);

            return productModel;
        }

        public override async Task GetAllProducts(GetAllProductsRequest request,
            IServerStreamWriter<ProductModel> responseStream,
            ServerCallContext context)
        {
            var products = await _context.Product.ToListAsync();

            foreach (var product in products)
            {
                var productModel = _mapper.Map<ProductModel>(product);

                await responseStream.WriteAsync(productModel);
            }
        }

        public override async Task<ProductModel> AddProduct(AddProductRequest request, 
            ServerCallContext context)
        {
            var product = _mapper.Map<Product>(request.Product);

            _context.Product.Add(product);

            await _context.SaveChangesAsync();

            var productModel = _mapper.Map<ProductModel>(product);

            return productModel;
        }
    }
}