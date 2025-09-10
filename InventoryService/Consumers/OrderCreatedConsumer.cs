using MassTransit;
using InventoryService.Data;
using InventoryService.Consumers;
using Messages;
using Microsoft.AspNetCore.Http.HttpResults;

namespace InventoryService.Consumers
{
    public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
    {
        private readonly InventoryDbContext _context;

        public OrderCreatedConsumer(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            var message = context.Message;

            var product = await _context.Products.FindAsync(message.ProductId);
            if (product == null)
            {
                //logar produto não encontrado
                return;
            }

            if (product.Stock < message.Quantity)
            {
                //logar estoque insuficiente
                return;
            }

            product.Stock -= message.Quantity;
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }
    }
}
