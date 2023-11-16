using System.ComponentModel.DataAnnotations;

namespace NetCoreConfMad2023_API.Models
{
    public class Product
    {
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public double Price { get; set; }
    }
}
