namespace Play.Trading.Service.Settings;

public class QueueSettings
{
    public string GrantItemsQueueAddress { get; init; }
    public string DebitGillQueueAddress { get; init; }
    public string SubstractItemsQueueAddress { get; init; }
}