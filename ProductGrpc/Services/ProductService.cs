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
                throw new RpcException(new Status(StatusCode.NotFound, 
                    $"Product ID = {request.ProductId} not found"));
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

            _logger.LogInformation($"Product successfully added on server side. Id: {product.Id} - Description: {product.Description}")

            var productModel = _mapper.Map<ProductModel>(product);

            return productModel;
        }

        public override async Task<ProductModel> UpdateProduct(UpdateProductRequest request,
            ServerCallContext context)
        {
            var product = _mapper.Map<Product>(request.Product);

            bool existProduct = await _context.Product.AnyAsync(p => p.Id == product.Id);

            if (!existProduct)
            {
                throw new RpcException(new Status(StatusCode.NotFound,
                    $"Product ID = {request.Product.ProductId} not found"));
            }

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            var productModel = _mapper.Map<ProductModel>(product);

            return productModel;
        }

        public override async Task<DeleteProductResponse> DeleteProduct(DeleteProductRequest request, 
            ServerCallContext context)
        {
            var product = await _context.Product.FindAsync(request.ProductId);

            if (product is null)
            {
                throw new RpcException(new Status(StatusCode.NotFound,
                    $"Product ID = {request.ProductId} not found"));
            }

            _context.Product.Remove(product);

            var deleteCount = await _context.SaveChangesAsync();

            var response = new DeleteProductResponse
            {
                Success = deleteCount > 0
            };

            return response;
        }

        public override async Task<InsertBulkProductResponse> InsertBulkProduct(IAsyncStreamReader<ProductModel> requestStream,
            ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                var producrt = _mapper.Map<Product>(requestStream.Current);

                _context.Product.Add(producrt);
            }

            var inserCount = _context.SaveChanges();

            var response = new InsertBulkProductResponse
            {
                Success = inserCount > 0,
                InsertCount = inserCount
            };

            return response;
        }
    }
}