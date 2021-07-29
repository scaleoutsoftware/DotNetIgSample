using System;
using System.Collections.Generic;
using System.Text;
using MessagePack;
using Microsoft.Extensions.Logging;
using Scaleout.Client;
using Scaleout.InvocationGrid.Hosting;
using ShoppingCart;

namespace ShoppingCartIG
{
    class Startup : IInvocationGridStartup
    {
        /// <summary>
        /// Configures an invocation grid when it first starts up. Typically used to
        /// initialize Scaleout.Client caches and register invocation handlers.
        /// </summary>
        /// <param name="gridConnection">Connection to the ScaleOut service.</param>
        /// <param name="logger">ILogger instance.</param>
        /// <param name="startupParam">Optional, arbitrary payload sent by IG launch call.</param>
        /// <param name="igName">Name of this invocation grid.</param>
        public void Configure(GridConnection gridConnection, ILogger logger, byte[] startupParam, string igName)
        {
            // Configure the cache use to access shopping cart objects.
            var cacheBuilder = new CacheBuilder<string, Cart>("Shopping Carts", gridConnection);

            // For tips on optimizing the cache in IG applications, see:
            // https://static.scaleoutsoftware.com/docs/dotnet_client/articles/pmi/using/handler/pmi_performance.html

            cacheBuilder.SetSerialization(
                                    (cart, stream) => MessagePackSerializer.Serialize(stream, cart),
                                    (stream) => MessagePackSerializer.Deserialize<Cart>(stream));

            cacheBuilder.SetClientCache("Random-MaxMemory", capacity: 1000, partitionCount: 0);
            cacheBuilder.SetKeystringCacheSize(100_000);

            var cache = cacheBuilder.Build();

            // Register the TotalBackorderedValue PMI handler class.
            ServiceEvents.SetInvokeHandler(cache, new TotalBackorderedValue(logger));
        }
    }
}
