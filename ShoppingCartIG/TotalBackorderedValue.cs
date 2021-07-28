using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MessagePack;
using Scaleout.Client;
using Scaleout.Client.MethodInvocation;
using ShoppingCart;

namespace ShoppingCartIG
{
    /// <summary>
    /// PMI Reduce handler class that finds the total value of backordered
    /// items in user shopping carts, expressed as a decimal.
    /// </summary>
    class TotalBackorderedValue : Reduce<string, Cart, decimal>
    {
        readonly ILogger _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">ILogger instance.</param>
        public TotalBackorderedValue(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Initializes a thread-local result accumulator.
        /// </summary>
        /// <returns>Decimal value of zero.</returns>
        /// <remarks>
        /// The PMI engine uses multiple threads on multiple servers to evaluate objects 
        /// in a cache. Each thread maintains its own thread-local accumulated result 
        /// value to minimize locking overhead.
        /// </remarks>
        public override decimal AccumulatorFactory() => decimal.Zero;


        /// <summary>
        /// Evaluates a shopping cart, adding the value of a cart's backordered items
        /// to the accumulated result.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="accumulator"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override decimal Evaluate(string key, 
                                         decimal accumulator, 
                                         OperationContext<string, Cart> context)
        {
            // Use the cache in the PMI context to retrieve the object being evaluated:
            var readOptions = new ReadOptions(ClientCacheUsage.ReturnCachedReference, GeoServerReadMode.None, context.FastReadVersion);
            var readResponse = context.Cache.Read(key, readOptions);

            switch (readResponse.Result)
            {
                case ServerResult.Retrieved:
                    var cart = readResponse.Value;
                    decimal backorderedVal = (from item in cart.Items
                                              where item.Backordered == true
                                              select item.Price * item.Quantity)
                                             .Sum();
                                  
                    return accumulator + backorderedVal;
                case ServerResult.NotFound:
                    _logger.LogInformation("{key} removed by another client during PMI operation.", key);
                    return accumulator;
                default:
                    // Throw an exception here if you'd like to return UnhandledExceptionInCallback
                    // to the Cache.Invoke caller.
                    _logger.LogWarning("Unexpected error {result} reading {key}.", readResponse.Result, key);
                    return accumulator;
            }
        }

        /// <summary>
        /// Combines result objects from different threads or machines.
        /// </summary>
        /// <param name="result1">First result.</param>
        /// <param name="result2">Second result.</param>
        /// <returns>Merge result.</returns>
        public override decimal MergeFinal(decimal result1, decimal result2) => 
            result1 + result2;

        /// <summary>
        /// Deserializes a result object using MessagePack.
        /// </summary>
        /// <param name="stream">Stream containing serialized result.</param>
        /// <returns>Decimal.</returns>
        public override decimal DeserializeResult(Stream stream) => 
            MessagePackSerializer.Deserialize<decimal>(stream);

        /// <summary>
        /// Serializes a result to a stream using MessagePack.
        /// </summary>
        /// <param name="result">Decimal result.</param>
        /// <param name="stream">Stream to write the serialized result to.</param>
        public override void SerializeResult(decimal result, Stream stream) => 
            MessagePackSerializer.Serialize(stream, result);
    }
}
