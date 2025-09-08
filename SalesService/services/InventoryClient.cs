using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace SalesService.Services
{
    public class InventoryClient
    {
        private readonly HttpClient _httpClient;

        public InventoryClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ProductDto?> GetProductAsync(int id, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            return await _httpClient.GetFromJsonAsync<ProductDto>($"api/products/{id}");
        }

        public async Task<bool> DecreaseStockAsync(int productId, int quantity, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var updateStockRequest = new { quantity };
            var response = await _httpClient.PostAsJsonAsync(
                $"api/products/{productId}/decrease-stock",
                updateStockRequest
            );

            return response.IsSuccessStatusCode;
        }
    }

    // DTO pode ficar aqui ou separado também
    public class ProductDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }

}
