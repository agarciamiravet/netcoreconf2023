using System.Text.Json.Serialization;

namespace NetCoreConfMad2023_API.Models
{
    public class ShoppingCart
    {
        [JsonPropertyName("userName")]
        public string UserName { get; set; }

        [JsonPropertyName("totalPrice")]
        public double TotalPrice { get; set; }

        [JsonPropertyName("items")]
        public List<ShoppingCartItem> Items { get; set; } = new List<ShoppingCartItem>();
        
        public ShoppingCart()
        {
        }
        public ShoppingCart(string userName)
        {
            UserName = userName;
        }
    }
}
