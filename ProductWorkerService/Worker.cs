using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProductGrpc.Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProductWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;

        public Worker(ILogger<Worker> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var taskInterval = _configuration.GetValue<int>("WorkerService:TaskInterval");
            var serverUrl = _configuration.GetValue<string>("WorkerService:ServerUrl");
            var productName = _configuration.GetValue<string>("WorkerService:ProductName");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                using var channel = GrpcChannel.ForAddress(serverUrl);
                var client = new ProductProtoService.ProductProtoServiceClient(channel);

                Console.WriteLine("AddProductAsync started..");

                var addProductResponse = await client.AddProductAsync(
                        new AddProductRequest
                        {
                            Product = new ProductModel
                            {
                                Name = productName + DateTimeOffset.Now,
                                Description = "Product added from client",
                                Price = 312,
                                Status = ProductStatus.Instock,
                                CreatedTime = Timestamp.FromDateTime(DateTime.UtcNow)
                            }
                        });

                Console.WriteLine("AddProduct Response: " + addProductResponse.ToString());

                await Task.Delay(taskInterval, stoppingToken);
            }
        }
    }
}