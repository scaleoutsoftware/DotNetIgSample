using MessagePack;
using System;
using System.Collections.Generic;

// Required NuGet package: MessagePack.Annotations

namespace ShoppingCart
{
    /// <summary>
    /// Shopping cart DTO that is stored in the ScaleOut service.
    /// </summary>
    [MessagePackObject]
    public class Cart
    {
        [Key(0)]
        public string UserId { get; set; }

        [Key(1)]
        public List<CartItem> Items { get; set; }
    }
}
