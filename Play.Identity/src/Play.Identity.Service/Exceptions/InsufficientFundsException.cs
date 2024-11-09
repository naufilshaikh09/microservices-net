namespace Play.Identity.Service.Exceptions;

[Serializable]
public class InsufficientFundsException : Exception
{
    public InsufficientFundsException(Guid userId, Decimal gilToDebit)
        : base($"Not enough git to debit {gilToDebit} Gil from user {userId}")
    {
        UserId = userId;
        GilToDebit = gilToDebit;
    }
    
    public Guid UserId { get; }
    public Decimal GilToDebit { get; }
}