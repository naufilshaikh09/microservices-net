namespace Play.Identity.Service.Exceptions;

[Serializable]
public class UnknownUserException : Exception
{
    public UnknownUserException(Guid userId) :
        base($"Unknown user '{userId}'")
    {
        UserId = userId;
    }

    public Guid UserId { get; }
}