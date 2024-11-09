namespace Play.Trading.Service.Exceptions;

[Serializable]
public class UnknownItemException : Exception
{
    public UnknownItemException(Guid itemId) :
        base($"Unknown item '{itemId}'")
    {
        ItemId = itemId;
    }

    public Guid ItemId { get; set; }
}