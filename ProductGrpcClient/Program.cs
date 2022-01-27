using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using ProductGrpc.Protos;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductGrpcClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Waiting server..");
            Thread.Sleep(1800);

            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new ProductProtoService.ProductProtoServiceClient(channel);

            await GetProductAsync(client);

            await GetAllProducts(client);

            await AddProductAsync(client);

            await UpdateProductAsync(client);

            await DeleteProductAsync(client);

            await GetAllProducts(client);
            await InsertBulkProduct(client);
            await GetAllProducts(client);

            Console.ReadKey();
        }

        private static async Task InsertBulkProduct(ProductProtoService.ProductProtoServiceClient client)
        {
            Console.WriteLine("InsertBulkProduct started..");

            using var clientBulk = client.InsertBulkProduct();

            for (int i = 0; i < 3; i++)
            {
                var productModel = new ProductModel
                {
                    Name = $"Product {i}",
                    Description = "Bulk inserted product",
                    Price = 100 + i,
                    Status = ProductStatus.Instock,
                    CreatedTime = Timestamp.FromDateTime(DateTime.UtcNow)
                };

                await clientBulk.RequestStream.WriteAsync(productModel);
            }

            await clientBulk.RequestStream.CompleteAsync();

            var responseBulk = await clientBulk;

            Console.WriteLine($"Status: {responseBulk.Success}. Insert Count: {responseBulk.InsertCount}.");
        }

        private static async Task DeleteProductAsync(ProductProtoService.ProductProtoServiceClient client)
        {
            Console.WriteLine("DeleteProductAsync started..");

            var deleteResponse = await client.DeleteProductAsync(
                new DeleteProductRequest
                {
                    ProductId = 2
                });

            Console.WriteLine($"Delete operation status: {deleteResponse.Success.ToString()}");
            Thread.Sleep(1000);
        }

        private static async Task UpdateProductAsync(ProductProtoService.ProductProtoServiceClient client)
        {
            Console.WriteLine("UpdateProductAsync started..");

            var updateResponse = await client.UpdateProductAsync(
                new UpdateProductRequest
                {
                    Product = new ProductModel
                    {
                        ProductId = 1,
                        Name = "Prod 1.1",
                        Description = "Description prod 1 - updated",
                        Price = 928,
                        Status = ProductStatus.Low,
                        CreatedTime = Timestamp.FromDateTime(DateTime.UtcNow)
                    }
                });

            Console.WriteLine("Updated Product: " + updateResponse.ToString());
        }

        private static async Task GetAllProducts(ProductProtoService.ProductProtoServiceClient client)
        {
            Console.WriteLine("GetAllProducts started..");

            using var clientData = client.GetAllProducts(new GetAllProductsRequest());

            await foreach (var responseData in clientData.ResponseStream.ReadAllAsync())
            {
                Console.WriteLine(responseData);
            }
        }

        private static async Task GetProductAsync(ProductProtoService.ProductProtoServiceClient client)
        {
            Console.WriteLine("GetProductAsync started..");

            var response = await client.GetProductAsync(
                new GetProductRequest
                {
                    ProductId = 1
                });

            Console.WriteLine("GetProductAsync Response: " + response.ToString());
        }

        private static async Task AddProductAsync(ProductProtoService.ProductProtoServiceClient client)
        {
            Console.WriteLine("AddProductAsync started..");

            var addProductResponse = await client.AddProductAsync(
                    new AddProductRequest
                    {
                        Product = new ProductModel
                        {
                            Name = "Product Client",
                            Description = "Product added from client",
                            Price = 312,
                            Status = ProductStatus.Instock,
                            CreatedTime = Timestamp.FromDateTime(DateTime.UtcNow)
                        }
                    });

            Console.WriteLine("AddProduct Response: " + addProductResponse.ToString());
        }
    }
}
