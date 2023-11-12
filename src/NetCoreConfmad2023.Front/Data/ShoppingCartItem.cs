using System.Text.Json.Serialization;

namespace NetCoreConfMad2023_API.Models
{
    public class ShoppingCartItem
    {
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
        [JsonPropertyName("price")]
        public double Price { get; set; }

        [JsonPropertyName("productId")]
        public string ProductId { get; set; }

        [JsonPropertyName("productName")]
        public string ProductName { get; set; }
    }
}
