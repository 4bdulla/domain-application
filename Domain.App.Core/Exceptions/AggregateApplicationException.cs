using Domain.App.Core.Exceptions.Abstraction;


namespace Domain.App.Core.Exceptions;

public abstract class AggregateApplicationException : AggregateException, IApplicationException
{
    public AggregateApplicationException()
    {
        this.ErrorCode = GetType().Name.Replace(nameof(Exception), string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    public AggregateApplicationException(string message, params Exception[] innerExceptions) : base(message, innerExceptions)
    {
        this.ErrorCode = GetType().Name.Replace(nameof(Exception), string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    public string ErrorCode { get; set; }
}