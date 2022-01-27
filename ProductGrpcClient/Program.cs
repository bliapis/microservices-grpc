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

            Console.ReadKey();
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
