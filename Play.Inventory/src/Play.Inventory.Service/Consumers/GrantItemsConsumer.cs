using MassTransit;
using Play.Common;
using Play.Inventory.Contracts;
using Play.Inventory.Service.Entities;
using Play.Inventory.Service.Exceptions;

namespace Play.Inventory.Service.Consumers;

public class GrantItemsConsumer : IConsumer<GrantItems>
{
    private readonly IRepository<InventoryItem> inventoryItemsRepository;
    private readonly IRepository<CatalogItem> catalogItemsRepository;
    
    public GrantItemsConsumer(
        IRepository<InventoryItem> inventoryItemsRepository, 
        IRepository<CatalogItem> catalogItemsRepository)
    {
        this.inventoryItemsRepository = inventoryItemsRepository;
        this.catalogItemsRepository = catalogItemsRepository;
    }
    
    public async Task Consume(ConsumeContext<GrantItems> context)
    {
        var message = context.Message;
        
        var item = await catalogItemsRepository.GetAsync(message.CatalogItemId);
        
        if (item == null)
        {
            throw new UnknownItemException(message.CatalogItemId);
        }
        
        var inventoryItem = await inventoryItemsRepository.GetAsync(item =>
            item.UserId == message.UserId && item.CatalogItemId == message.CatalogItemId);
        
        if (inventoryItem == null)
        {
            inventoryItem = new InventoryItem
            {
                UserId = message.UserId,
                CatalogItemId = message.CatalogItemId,
                Quantity = message.Quantity,
                AcquiredDate = DateTimeOffset.UtcNow
            };
            
            inventoryItem.MessagedIds.Add(context.MessageId.Value);
            
            await inventoryItemsRepository.CreateAsync(inventoryItem);
        }
        else
        {
            if(inventoryItem.MessagedIds.Contains(context.MessageId.Value))
            {
                await context.Publish(new InventoryItemsGranted(message.CorrelationId));
                return;
            }
            
            inventoryItem.Quantity += message.Quantity;
            inventoryItem.MessagedIds.Add(context.MessageId.Value);
            await inventoryItemsRepository.UpdateAsync(inventoryItem);
        }

        
        var itemsGrantedTask = context.Publish(new InventoryItemsGranted(message.CorrelationId));
        var inventoryUpdatedTask = context.Publish(new InventoryItemUpdated(
            inventoryItem.UserId, 
            inventoryItem.CatalogItemId,
            inventoryItem.Quantity));
        
        await Task.WhenAll(itemsGrantedTask, inventoryUpdatedTask);
    }
}