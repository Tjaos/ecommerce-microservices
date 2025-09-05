using InventoryService.Data;
using InventoryService.DTOs;
using InventoryService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Controllers
{
    [ApiController]
    [Route("api/v1/inventory")]
    [Authorize] // Só pode acessar com JWT válido
    public class ProductsController : ControllerBase
    {
        private readonly InventoryDbContext _context;

        public ProductsController(InventoryDbContext context)
        {
            _context = context;
        }

        // GET: api/products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            return await _context.Products.ToListAsync();
        }

        // GET: api/products/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            return product;
        }

        // POST: api/products
        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetProducts), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, Product product)
        {
            var productDb = await _context.Products.FindAsync(id);

            if (productDb == null)
            {
                return NotFound("Produto não encontrado");
            }

            productDb.Name = product.Name;
            productDb.Description = product.Description;
            productDb.Price = product.Price;
            productDb.Stock = product.Stock;

            _context.Products.Update(productDb);
            await _context.SaveChangesAsync();
            return Ok(productDb);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var productDb = await _context.Products.FindAsync(id);
            if (productDb == null)
            {
                return StatusCode(422, "Produto não encontrado");
            }

            _context.Products.Remove(productDb);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{id}/decrease-stock")]
        public async Task<IActionResult> DecreaseStock(int id, [FromBody] UpdateStockRequest request)
        {
            if (request.Quantity <= 0)
            {
                return StatusCode(422, "Quantidade não pode ser negativa!");
            }

            var productDb = await _context.Products.FindAsync(id);
            if (productDb == null)
            {
                return NotFound("Produto não encontrado");
            }

            if (productDb.Stock < request.Quantity)
            {
                return BadRequest("Estoque insuficiente");
            }

            productDb.Stock -= request.Quantity;
            _context.Products.Update(productDb);
            await _context.SaveChangesAsync();
            return Ok(productDb);
        }

    }
}
