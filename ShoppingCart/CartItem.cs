using MessagePack;

// Required NuGet package: MessagePack.Annotations

namespace ShoppingCart
{
    /// <summary>
    /// Shopping cart item DTO.
    /// </summary>
    [MessagePackObject]
    public class CartItem
    {
        [Key(0)]
        public string ProductName { get; set; }

        [Key(1)]
        public int Quantity { get; set; }

        [Key(2)]
        public decimal Price { get; set; }

        [Key(3)]
        public bool Backordered { get; set; }
    }
}
