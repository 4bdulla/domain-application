using Domain.App.Core.Exceptions.Abstraction;


namespace Domain.App.Core.Exceptions;

public class IntegrationException : Exception, IApplicationException
{
    public IntegrationException() { }

    public IntegrationException(string message) : base(message) { }

    public IntegrationException(string clientName, string resultCode, string resultDescription)
        : base($"{clientName} failed with ResultCode: {resultCode} and ResultDescription: {resultDescription}")
    {
        this.ErrorCode = resultCode;
        this.ResultDescription = resultDescription;
    }

    public IntegrationException(string clientName, string resultCode, string resultDescription, Exception exception)
        : base($"{clientName} failed with ResultCode: {resultCode} and ResultDescription: {resultDescription}", exception)
    {
        this.ErrorCode = resultCode;
        this.ResultDescription = resultDescription;
    }

    public string ErrorCode { get; set; }

    public string ResultDescription { get; set; }
}