namespace TesorosChoco.Domain.Exceptions;

/// <summary>
/// Base class for all domain-specific exceptions
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
    
    protected DomainException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when an entity is not found
/// </summary>
public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityName, object key) 
        : base($"{entityName} with key '{key}' was not found.")
    {
    }
}

/// <summary>
/// Exception thrown when a business rule is violated
/// </summary>
public class BusinessRuleViolationException : DomainException
{
    public BusinessRuleViolationException(string rule) 
        : base($"Business rule violation: {rule}")
    {
    }
}

/// <summary>
/// Exception thrown when trying to create a duplicate entity
/// </summary>
public class DuplicateEntityException : DomainException
{
    public DuplicateEntityException(string entityName, string propertyName, object value) 
        : base($"{entityName} with {propertyName} '{value}' already exists.")
    {
    }
}

/// <summary>
/// Exception thrown when an entity is in an invalid state
/// </summary>
public class InvalidEntityStateException : DomainException
{
    public InvalidEntityStateException(string entityName, string reason) 
        : base($"{entityName} is in an invalid state: {reason}")
    {
    }
}
