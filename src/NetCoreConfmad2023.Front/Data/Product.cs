using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NetCoreConfmad2023.Front.Data
{
    public class Product
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("price")]
        public double Price { get; set; }
    }
}
