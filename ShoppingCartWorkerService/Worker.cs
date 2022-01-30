using Grpc.Core;
using Grpc.Net.Client;
using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProductGrpc.Protos;
using ShoppingCartGrpc.Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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

            while (!stoppingToken.IsCancellationRequested)
            {
                var taskInterval = _configuration.GetValue<int>("WorkerService:TaskInterval");
                var shoppingCartServerUrl = _configuration.GetValue<string>("WorkerService:ShoppingCartServerUrl");
                var productServerUrl = _configuration.GetValue<string>("WorkerService:ProductServerUrl");
                var userName = _configuration.GetValue<string>("WorkerService:UserName");

                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                #region Create ShoppingCartClient
                using var shoppingCartChannel = GrpcChannel.ForAddress(shoppingCartServerUrl);

                var shoppingCartClient = new ShoppingCartProtoService.ShoppingCartProtoServiceClient(shoppingCartChannel);
                #endregion

                #region Get Token and create authorization headers
                var token = await GetTokenFromIdentityServerAsync();

                var headers = new Metadata();

                headers.Add("Authorization", $"Bearer {token}");
                #endregion

                var shoppingCartModel = await GetOrCreateShoppingCartAsync(shoppingCartClient, userName, headers);

                #region Create ShoppingProductClient
                using var productChannel = GrpcChannel.ForAddress(productServerUrl);

                var productClient = new ProductProtoService.ProductProtoServiceClient(productChannel);
                #endregion


                _logger.LogInformation("AddProductsIntoShoppingCart starting...");
                
                using var shoppingClientStream = shoppingCartClient.AddItemIntoShoppingCart(headers);
                
                using var clientData = productClient.GetAllProducts(new GetAllProductsRequest());
                
                await foreach (var responseData in clientData.ResponseStream.ReadAllAsync())
                {
                    _logger.LogInformation("GetAllProducts Response Stream {responseData}", responseData);
                
                    var addNewShoppingCartItem = new AddItemIntoShoppingCartRequest
                    {
                        Username = userName,
                        DiscountCode = "COD100",
                        NewCartItem = new ShoppingCartItemModel
                        {
                            ProductId = responseData.ProductId,
                            Productname = responseData.Name,
                            Price = responseData.Price,
                            Color = "Default black",
                            Quantity = 1
                        }
                    };
                
                    await shoppingClientStream.RequestStream.WriteAsync(addNewShoppingCartItem);
                
                    _logger.LogInformation("ShoppingCart Client Stream Added New Item: {addNewShoppingCartItem}", addNewShoppingCartItem);
                }
                
                await shoppingClientStream.RequestStream.CompleteAsync();


                await Task.Delay(taskInterval, stoppingToken);
            }
        }

        private async Task<string> GetTokenFromIdentityServerAsync()
        {
            var identityServerUrl = _configuration.GetValue<string>("WorkerService:IdentityServerUrl");

            var client = new HttpClient();

            var disco = await client.GetDiscoveryDocumentAsync(identityServerUrl);

            if (disco.IsError)
            {
                Console.WriteLine(disco.Error);
                return default(string);
            }

            var tokenReponse = await client.RequestClientCredentialsTokenAsync(
                new ClientCredentialsTokenRequest
                {
                    Address = disco.TokenEndpoint,
                    ClientId = "ShoppingCartClient",
                    ClientSecret = "secret",
                    Scope = "ShoppingCartAPI"
                });

            if (tokenReponse.IsError)
            {
                Console.WriteLine(tokenReponse.Error);

                return default(string);
            }

            return tokenReponse.AccessToken;
        }

        private async Task<ShoppingCartModel> GetOrCreateShoppingCartAsync(ShoppingCartProtoService.ShoppingCartProtoServiceClient shoppingCartClient,
            string userName,
            Metadata headers)
        {
            ShoppingCartModel shoppingCartModel = null;

            var shoppingCartStatus = StatusCode.OK;

            try
            {
                _logger.LogInformation("GetShoppingCartAsync starting..");

                shoppingCartModel = await shoppingCartClient.GetShoppingCartAsync(
                    new GetShoppingCartRequest
                    {
                        Username = userName
                    },
                    headers);

                _logger.LogInformation("GetShoppingCartAsync Response: {shoppingCartModel}", shoppingCartModel);
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
                    new ShoppingCartModel { Username = userName },
                    headers);
            }

            _logger.LogInformation("ShoppingCartResponse: " + shoppingCartModel.ToString());

            return shoppingCartModel;
        }
    }
}
