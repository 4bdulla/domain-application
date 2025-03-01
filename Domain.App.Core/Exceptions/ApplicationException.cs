using Domain.App.Core.Exceptions.Abstraction;

namespace Domain.App.Core.Exceptions;

public abstract class ApplicationException : Exception, IApplicationException
{
    public ApplicationException() : base()
    {
        this.ErrorCode = base.GetType().Name.Replace(nameof(Exception), string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    public ApplicationException(string message) : base(message)
    {
        this.ErrorCode = base.GetType().Name.Replace(nameof(Exception), string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    public string ErrorCode { get; set; }
}