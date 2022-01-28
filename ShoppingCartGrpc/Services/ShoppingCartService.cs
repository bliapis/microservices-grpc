using AutoMapper;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShoppingCartGrpc.Data;
using ShoppingCartGrpc.Models;
using ShoppingCartGrpc.Protos;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ShoppingCartGrpc.Services
{
    public class ShoppingCartService : ShoppingCartProtoService.ShoppingCartProtoServiceBase
    {
        private readonly ShoppingCartContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ShoppingCartService> _logger;

        public ShoppingCartService(
            ShoppingCartContext context,
            IMapper mapper,
            ILogger<ShoppingCartService> logger)
        {
            _context = context;
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task<ShoppingCartModel> GetShoppingCart(GetShoppingCartRequest request, 
            ServerCallContext context)
        {
            var shoppingCart = await _context.ShoppingCart
                .FirstOrDefaultAsync(s => s.UserName == request.Username);

            if (shoppingCart is null) 
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"ShoppingCart with username = {request.Username}"));
            }

            var shoppingCartModel = _mapper.Map<ShoppingCartModel>(shoppingCart);

            return shoppingCartModel;
        }

        public override async Task<ShoppingCartModel> CreateShoppingCart(ShoppingCartModel request, 
            ServerCallContext context)
        {
            var shoppingCart = _mapper.Map<ShoppingCart>(request);

            var existShoppingCart = await _context.ShoppingCart
                    .AnyAsync(s => s.UserName == shoppingCart.UserName);

            if (existShoppingCart)
            {
                _logger.LogError($"ShoppingCart with username: {request.Username} already exists.");

                throw new RpcException(new Status(StatusCode.AlreadyExists, $"ShoppingCart with username: {request.Username} already exists."));
            }

            _context.ShoppingCart.Add(shoppingCart);

            await _context.SaveChangesAsync();

            var shoppingCartModel = _mapper.Map<ShoppingCartModel>(shoppingCart);

            return shoppingCartModel;
        }

        public override async Task<RemoveItemFromShoppingCartResponse> RemoveItemFromShoppingCart(RemoveItemFromShoppingCartRequest request, 
            ServerCallContext context)
        {
            var shoppingCart = await _context.ShoppingCart
                    .FirstOrDefaultAsync(s => s.UserName == request.Username);

            if (shoppingCart is null)
            {
                _logger.LogError($"ShoppingCart with username: {request.Username} does not exists.");

                throw new RpcException(new Status(StatusCode.NotFound, $"ShoppingCart with username: {request.Username} does not exists."));
            }

            var removeCartItem = shoppingCart.Items
                    .FirstOrDefault(i => i.ProductId == request.RemoveCartItem.ProductId);

            if (removeCartItem is null)
            {
                _logger.LogError($"ShoppingCartItem with ProductId: {request.RemoveCartItem.ProductId} does not exists.");

                throw new RpcException(new Status(StatusCode.NotFound, $"ShoppingCartItem with ProductId: {request.RemoveCartItem.ProductId} does not exists."));
            }

            shoppingCart.Items.Remove(removeCartItem);

            var removeCount = await _context.SaveChangesAsync();

            var response = new RemoveItemFromShoppingCartResponse
            {
                Success = removeCount > 0
            };

            return response;
        }

        public override async Task<AddItemIntoShoppingCartResponse> AddItemIntoShoppingCart(IAsyncStreamReader<AddItemIntoShoppingCartRequest> requestStream,
            ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                var shoppingCart = await _context.ShoppingCart
                    .FirstOrDefaultAsync(s => s.UserName == requestStream.Current.Username);

                if (shoppingCart is null)
                {
                    _logger.LogError($"ShoppingCart with username: {requestStream.Current.Username} does not exists.");

                    throw new RpcException(new Status(StatusCode.NotFound, $"ShoppingCart with username: {requestStream.Current.Username} does not exists."));
                }

                var newAddedCartItem = _mapper.Map<ShoppingCartItem>(requestStream.Current.NewCartItem);

                var cartItem = shoppingCart.Items.FirstOrDefault(p => p.ProductId == newAddedCartItem.ProductId);

                if (cartItem != null)
                {
                    cartItem.Quantity++;
                }
                else
                {
                    //TODO: grpc call discount service - check discount and calculate the item last price
                    float discount = 100;
                    newAddedCartItem.Price -= discount;

                    shoppingCart.Items.Add(newAddedCartItem);
                }
            }

            var insertCount = await _context.SaveChangesAsync();

            var response = new AddItemIntoShoppingCartResponse
            {
                Success = insertCount > 0,
                InsertCount = insertCount
            };

            return response;
        }
    }
}