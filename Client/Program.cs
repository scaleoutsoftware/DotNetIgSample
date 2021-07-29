using System;
using Scaleout.Client;
using MessagePack;
using ShoppingCart;
using System.Collections.Generic;
using System.Text;

namespace Client
{
    class Program
    {
        const string SCALEOUT_CONNSTR = "bootstrapGateways=localhost:721";
        const int CART_COUNT = 5000;
        const int MAX_CART_ITEMS = 2;

        /// <summary>
        /// Adds 
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var cache = GetCache();
            AddCartsToCache(cache);

            var invokeResponse = cache.Invoke("TotalBackorderedValue",
                                              param: null,
                                              invocationGrid: "ShoppingCartIG");
            
            switch (invokeResponse.Result)
            {
                case ServerResult.InvokeCompleted:
                    decimal backorderdValue = MessagePackSerializer.Deserialize<decimal>(invokeResponse.ResultObject);
                    Console.WriteLine($"Value of backordered items in carts: {backorderdValue}");
                    break;
                case ServerResult.UnhandledExceptionInCallback:
                    Console.WriteLine("Unhandled exception(s) were thrown from invocation handler.");
                    string exceptionInfo = Encoding.UTF8.GetString(invokeResponse.ErrorData);
                    Console.WriteLine(exceptionInfo);
                    break;
                default:
                    Console.WriteLine($"Cache.Invoke returned unexpected {invokeResponse.Result}");
                    break;
            }
        }

        /// <summary>
        /// Connects to the ScaleOut service and gets a cache instance
        /// used to access shopping cart objects.
        /// </summary>
        /// <returns>
        /// A cache of Cart objects, keyed by a string (the User ID).
        /// </returns>
        static Cache<string, Cart> GetCache()
        {
            var gridConnection = GridConnection.Connect(SCALEOUT_CONNSTR);
            
            var cacheBuilder = new CacheBuilder<string, Cart>("Shopping Carts", gridConnection);
            cacheBuilder.SetSerialization(
                                    (cart, stream) => MessagePackSerializer.Serialize(stream, cart),
                                    (stream) => MessagePackSerializer.Deserialize<Cart>(stream));
            
            return cacheBuilder.Build();
        }


        /// <summary>
        /// Generates random shopping cart instances and adds them to the ScaleOut service.
        /// </summary>
        /// <param name="cache">Cache instance.</param>
        static void AddCartsToCache(Cache<string, Cart> cache)
        {
            string[]  products   = new[] { "Red sweater", "Brown shoes", "Blue Pants", "Silver Watch" };
            decimal[] prices     = new[] {        45.00m,        60.00m,       30.00m,           200m };
            bool[]    backorderd = new[] {          true,         false,        false,          false };

            Random rand = new Random();
            for (int i = 0; i < CART_COUNT; i++)
            {
                string userName = $"Shopper {i:D4}";
                int itemCount = rand.Next(MAX_CART_ITEMS) + 1;
                List<CartItem> items = new List<CartItem>(itemCount);
                for (int j = 0; j < itemCount; j++)
                {
                    int productIndex = rand.Next(products.Length);
                    CartItem item = new CartItem
                    {
                        ProductName = products[productIndex],
                        Price = prices[productIndex],
                        Quantity = 1,
                        Backordered = backorderd[productIndex]
                    };
                    items.Add(item);
                }
                Cart cart = new Cart { Items = items, UserId = userName };
                
                var response = cache.Add(cart.UserId, cart);

                if (response.Result != ServerResult.Added &&
                    response.Result != ServerResult.AlreadyExistsError)
                    throw new Exception($"Unexpected result {response.Result} returned from ScaleOut service");
            }
        }
    }
}