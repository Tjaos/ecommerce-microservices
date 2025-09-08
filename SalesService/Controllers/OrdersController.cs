using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesService.Data;
using SalesService.Models;
using SalesService.Services;

namespace SalesService.Controllers
{
    [ApiController]
    [Route("api/v1/sales")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly SalesDbContext _context;

        private readonly InventoryClient _inventoryClient;

        public OrdersController(SalesDbContext context, InventoryClient inventoryClient)
        {
            _context = context;
            _inventoryClient = inventoryClient;
        }

        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrder(Order order)
        {
            // Pegar o token JWT do header da requisição do usuário
            var accessToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            var product = await _inventoryClient.GetProductAsync(order.ProductId, accessToken);
            if (product == null)
                return NotFound("Produto não encontrado no estoque.");

            int orderQuantity = order.Items.Sum(i => i.Quantity);

            if (product.Stock < orderQuantity)
                return BadRequest("Estoque insuficiente.");

            var item = new OrderItem
            {
                ProductId = order.ProductId,
                Quantity = orderQuantity,
                Price = product.Price * orderQuantity
            };
            order.Items.Add(item);

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            bool stockUpdated = await _inventoryClient.DecreaseStockAsync(order.ProductId, orderQuantity, accessToken);
            if (!stockUpdated)
                return StatusCode(500, "Erro ao atualizar estoque no InventoryService.");

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
}