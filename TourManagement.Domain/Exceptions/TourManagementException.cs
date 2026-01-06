namespace TourManagement.Domain.Exceptions;

/// <summary>
/// Base exception for tour management domain
/// </summary>
public class TourManagementException : Exception
{
    public TourManagementException()
    {
    }

    public TourManagementException(string message) : base(message)
    {
    }

    public TourManagementException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when an entity is not found
/// </summary>
public class EntityNotFoundException : TourManagementException
{
    public EntityNotFoundException(string entityName, int id)
        : base($"{entityName} with ID {id} was not found.")
    {
    }
}
