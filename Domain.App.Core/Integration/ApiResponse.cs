namespace Domain.App.Core.Integration;

public static class ApiResponse
{
    /// <summary>
    ///     Creates <see cref="ApiResponse{T}" />
    /// </summary>
    /// <returns><see cref="ApiResponse{T}" /> with result code = 0 &amp; result description = "Success"</returns>
    public static ApiResponse<object> Ok() => new("0");

    /// <summary>
    ///     Creates <see cref="ApiResponse{T}" />
    /// </summary>
    /// <param name="payload">Payload of type <typeparamref name="T" /></param>
    /// <typeparam name="T">Type of the payload</typeparam>
    /// <returns>
    ///     <see cref="ApiResponse{T}" /> with result code = 0 &amp; result description = "Success" &amp; payload of type
    ///     <typeparamref name="T" />
    /// </returns>
    public static ApiResponse<T> Ok<T>(T payload) => new("0", string.Empty, payload);

    public static ApiResponse<object> Error(Exception ex, bool includeExceptionDetails = false) =>
        CreateResponse("1", ex, includeExceptionDetails);

    public static ApiResponse<object> CreateResponse(string resultCode, Exception exception, bool includeExceptionDetails)
    {
        return includeExceptionDetails
            ? new ApiResponse<object>(resultCode, exception.Message, new { exception.Message, exception.StackTrace })
            : new ApiResponse<object>(resultCode, exception.Message);
    }
}


public class ApiResponse<T>
{
    public ApiResponse(string resultCode, string resultDescription = "", T payload = default)
    {
        this.ResultCode = resultCode;
        this.Payload = payload;

        this.ResultDescription = resultCode == "0" ? "Success" : resultDescription;
    }


    public string ResultCode { get; set; }
    public string ResultDescription { get; set; }
    public T Payload { get; set; }
}