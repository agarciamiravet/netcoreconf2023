using Microsoft.AspNetCore.SignalR;
using NetCoreConfmad2023.Front.Data;
using NetCoreConfMad2023_API.Models;
using System.IO;
using System.Text;
using System.Text.Json;

namespace NetCoreConfmad2023.Front.Services
{
    public class CartService
    {
        private readonly CatalogService _catalogService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public CartService(CatalogService catalogService, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _catalogService = catalogService;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }
        public List<Product> SelectedItems { get; set; } = new();

        public async Task AddProductToCart(string productId)
        {
            var product = await _catalogService.GetProductById(productId);

            var shoppingCart = new ShoppingCart("default");

            //add current
            foreach(var selectedItem in SelectedItems)
            {
                shoppingCart.Items.Add(new ShoppingCartItem { ProductId = selectedItem.Id, ProductName = selectedItem.Name, Price = selectedItem.Price, Quantity = 1 });
            }

            //add new 
            shoppingCart.Items.Add(new ShoppingCartItem { ProductId = product.Id, ProductName = product.Name, Price = product.Price, Quantity = 1 });

            var jSonData = JsonSerializer.Serialize(shoppingCart);

            var webApiUrl = _configuration["ApiUrl"];

            var shoppingCartUrl = string.Format("{0}/api/ShoppingCart", webApiUrl);
            
            //var request = new HttpRequestMessage(HttpMethod.Put, shoppingCartUrl);

            var client = _httpClientFactory.CreateClient();

            var content = new StringContent(jSonData, Encoding.UTF8, "application/json");

            await client.PostAsync(shoppingCartUrl, content);

            SelectedItems.Add(product);

        }

        public async Task<List<Product>> GetBasketCart()
        {
            List<Product> products = new();
            var username = "default";
            var webApiUrl = _configuration["ApiUrl"];

            var shoppingCartUrl = string.Format("{0}/api/ShoppingCart/{1}", webApiUrl, username);

            var request = new HttpRequestMessage(HttpMethod.Get, shoppingCartUrl);

            var client = _httpClientFactory.CreateClient();

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();

                //StreamReader reader = new StreamReader(responseStream);
                //string text = reader.ReadToEnd();

                var result = await JsonSerializer.DeserializeAsync<ShoppingCart>(responseStream);

                if(result.Items !=null && result.Items.Count > 0)
                {
                    foreach(var basketItem in result.Items )
                    {
                        products.Add(new Product { Id = basketItem.ProductId, Name = basketItem.ProductName, Description = "", Price = basketItem.Price });
                    }                    
                }
            }

            return products;
        }
    }
}
