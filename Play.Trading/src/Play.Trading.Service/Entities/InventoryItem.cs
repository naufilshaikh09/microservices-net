using Play.Common;

namespace Play.Trading.Service.Entities;

public class InventoryItem : IEntity
{
    public Guid UserId { get; set; }
    public Guid CatalogItemId { get; set; }
    public int Quantity { get; set; }
    public Guid Id { get; set; }
}