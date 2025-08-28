using InventoryService.Data;
using InventoryService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
            var productBanco = await _context.Products.FindAsync(id);

            if (productBanco == null)
            {
                return NotFound("Produto não encontrado");
            }

            productBanco.Name = product.Name;
            productBanco.Description = product.Description;
            productBanco.Price = product.Price;
            productBanco.Stock = product.Stock;

            _context.Products.Update(productBanco);
            await _context.SaveChangesAsync();
            return Ok(productBanco);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var productBanco = await _context.Products.FindAsync(id);
            if (productBanco == null)
            {
                return NotFound("Produto não encontrado");
            }

            _context.Products.Remove(productBanco);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
