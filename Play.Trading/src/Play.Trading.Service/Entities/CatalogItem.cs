using Play.Common;

namespace Play.Trading.Service.Entities;

public class CatalogItem : IEntity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public Guid Id { get; set; }
}