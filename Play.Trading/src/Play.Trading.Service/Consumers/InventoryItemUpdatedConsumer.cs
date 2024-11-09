using MassTransit;
using Play.Common;
using Play.Inventory.Contracts;
using Play.Trading.Service.Entities;

namespace Play.Trading.Service.Consumers;

public class InventoryItemUpdatedConsumer : IConsumer<InventoryItemUpdated>
{
    private readonly IRepository<InventoryItem> _repository;

    public InventoryItemUpdatedConsumer(IRepository<InventoryItem> repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<InventoryItemUpdated> context)
    {
        var message = context.Message;
        var inventoyItem = await _repository.GetAsync(
            item => item.UserId == message.UserId && item.CatalogItemId == message.CatalogItemId);

        if (inventoyItem == null)
        {
            inventoyItem = new InventoryItem
            {
                UserId = message.UserId,
                CatalogItemId = message.CatalogItemId,
                Quantity = message.NewTotalQuantity
            };
            await _repository.CreateAsync(inventoyItem);
        }
        else
        {
            inventoyItem.Quantity = message.NewTotalQuantity;
            await _repository.UpdateAsync(inventoyItem);
        }
    }
}