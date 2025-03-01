namespace Domain.App.Core.Exceptions.Abstraction;

public interface IApplicationException
{
    string ErrorCode { get; set; }
}