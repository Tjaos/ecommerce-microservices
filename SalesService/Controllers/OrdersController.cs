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
            _httpClient = httpClientFactory.CreateClient();
        }

        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrder(Order order)
        {
            // Pegar o token JWT do header da requisição do usuário
            var accessToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            // Configurar o HttpClient para enviar o token ao InventoryService
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            // Fazer a requisição autenticada
            var inventoryResponse = await _httpClient.GetFromJsonAsync<ProductDto>(
                $"http://localhost:5236/api/products/{order.ProductId}");


            if (inventoryResponse == null)
                return NotFound("Produto não encontrado no estoque.");

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            if (inventoryResponse == null)
                return NotFound("Produto não encontrado no estoque.");

            if (inventoryResponse.Stock < order.Items.Sum(i => i.Quantity))
                return BadRequest("Estoque insuficiente.");

            // 2 - Criar pedido
            OrderItem item = new OrderItem
            {
                ProductId = order.ProductId,
                Quantity = order.Items.Sum(i => i.Quantity),
                Price = inventoryResponse.Price * order.Items.Sum(i => i.Quantity)
            };
            order.Items.Add(item);
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // 3 - Chamar InventoryService para atualizar estoque
            inventoryResponse.Stock -= order.Items.Sum(i => i.Quantity);
            await _httpClient.PutAsJsonAsync(
                $"http://localhost:5236/api/products/{order.ProductId}", inventoryResponse);

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