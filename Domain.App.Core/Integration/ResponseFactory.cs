namespace Domain.App.Core.Integration;

public static class ResponseFactory
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
    public static ApiResponse<T> Ok<T>(T payload) => new("0", payload: payload);

    public static ApiResponse<object> Error(Exception ex, bool includeExceptionDetails = false) =>
        CreateResponse("1", ex, includeExceptionDetails);

    public static ApiResponse<object> CreateResponse(string resultCode, Exception exception, bool includeExceptionDetails) =>
        includeExceptionDetails
            ? new ApiResponse<object>(resultCode, exception.Message, new { exception.Message, exception.StackTrace })
            : new ApiResponse<object>(resultCode, exception.Message);
}


public class ApiResponse<T>(string resultCode, string resultDescription = "Success", T payload = default)
{
    public string ResultCode { get; set; } = resultCode;
    public string ResultDescription { get; set; } = resultDescription;
    public T Payload { get; set; } = payload;
}