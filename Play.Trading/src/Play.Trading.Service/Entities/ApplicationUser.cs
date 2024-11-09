using Play.Common;

namespace Play.Trading.Service.Entities;

public class ApplicationUser : IEntity
{
    public decimal Gil { get; set; }
    public Guid Id { get; set; }
}