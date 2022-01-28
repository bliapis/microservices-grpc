using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ShoppingCartGrpc.Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ShoppingCartWorkerService
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
            Console.WriteLine("Waiting server..");
            Thread.Sleep(2000);

            var taskInterval = _configuration.GetValue<int>("WorkerService:TaskInterval");
            var shoppingCartServerUrl = _configuration.GetValue<string>("WorkerService:ShoppingCartServerUrl");
            var productServerUrl = _configuration.GetValue<string>("WorkerService:ProductServerUrl");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                using var shoppingCartChannel = GrpcChannel.ForAddress(shoppingCartServerUrl);

                var shoppingCartClient = new ShoppingCartProtoService.ShoppingCartProtoServiceClient(shoppingCartChannel);

                var shoppingCartModel = await GetOrCreateShoppingCartAsync(shoppingCartClient);

                await Task.Delay(taskInterval, stoppingToken);
            }
        }

        private async Task<ShoppingCartModel> GetOrCreateShoppingCartAsync(ShoppingCartProtoService.ShoppingCartProtoServiceClient shoppingCartClient)
        {
            ShoppingCartModel shoppingCartModel = null;

            var shoppingCartStatus = StatusCode.OK;

            var userName = _configuration.GetValue<string>("WorkerService:UserName");

            try
            {
                _logger.LogInformation("GetShoppingCartAsync starting..");

                shoppingCartModel = await shoppingCartClient.GetShoppingCartAsync(
                    new GetShoppingCartRequest
                    {
                        Username = userName
                    });
            }
            catch (RpcException ex)
            {
                if (ex.StatusCode == StatusCode.NotFound)
                {
                    shoppingCartStatus = StatusCode.NotFound;
                }
            }

            if (shoppingCartStatus == StatusCode.NotFound)
            {
                _logger.LogInformation("CreateShoppingCartAsync starting..");

                shoppingCartModel = await shoppingCartClient.CreateShoppingCartAsync(
                    new ShoppingCartModel { Username = userName });
            }

            _logger.LogInformation("ShoppingCartResponse: " + shoppingCartModel.ToString());

            return shoppingCartModel;
        }
    }
}
