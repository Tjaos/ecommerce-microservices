using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesService.Data;
using SalesService.Models;

namespace SalesService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly SalesDbContext _context;

        private readonly HttpClient _httpClient;

        public OrdersController(SalesDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient("InventoryClient");
        }

        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrder(Order order)
        {
            // Pegar o token JWT do header da requisição do usuário
            var accessToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            // Configurar o HttpClient para enviar o token ao InventoryService
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);


            // Buscar produto no InventoryService
            var product = await _httpClient.GetFromJsonAsync<ProductDto>(
                $"api/products/{order.ProductId}");

            if (product == null)
                return NotFound("Produto não encontrado no estoque.");

            int orderQuantity = order.Items.Sum(i => i.Quantity);

            if (product.Stock < orderQuantity)
                return BadRequest("Estoque insuficiente.");

            // Criar pedido com item vinculado ao produto
            OrderItem item = new OrderItem
            {
                ProductId = order.ProductId,
                Quantity = orderQuantity,
                Price = product.Price * orderQuantity
            };
            order.Items.Add(item);

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Chamar rota de atualização do estoque no InventoryService
            var updateStockRequest = new
            {
                quantity = orderQuantity
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"api/products/{order.ProductId}/decrease-stock",
                updateStockRequest
            );

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "Erro ao atualizar estoque no InventoryService.");
            }

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _context.Orders.Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();
            return order;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            return await _context.Orders.Include(o => o.Items).ToListAsync();
        }
    }

    public class ProductDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}