using NetCoreConfmad2023.Front.Data;
using System.Net.Http;
using System.Text.Json;

namespace NetCoreConfmad2023.Front.Services
{
    public class CatalogService
    {

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public CatalogService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<List<Product>> GetAllProducts()
        {
            List<Product> products = new();
            var webApiUrl = _configuration["ApiUrl"];
            var productUrl = string.Format("{0}/api/Product", webApiUrl);
            var request = new HttpRequestMessage(HttpMethod.Get, productUrl);

            var client = _httpClientFactory.CreateClient();

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();

                products = await JsonSerializer.DeserializeAsync<List<Product>>(responseStream);
            }

            return products;
        }

        public async Task<Product> GetProductById(string guid)
        {
            Product product = new();
            var webApiUrl = _configuration["ApiUrl"];
            var productUrl = string.Format("{0}/api/Product/{1}", webApiUrl,guid);
            var request = new HttpRequestMessage(HttpMethod.Get, productUrl);

            var client = _httpClientFactory.CreateClient();

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();

                product = await JsonSerializer.DeserializeAsync<Product>(responseStream);
            }

            return product;
        }

    }
}
