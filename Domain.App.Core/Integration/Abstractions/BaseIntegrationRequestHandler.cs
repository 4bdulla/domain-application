using System.Threading;
using Domain.App.Core.Exceptions;
using MediatR;
using Refit;

namespace Domain.App.Core.Integration.Abstractions;

public abstract class BaseIntegrationRequestHandler<TApi, TRequest> : IRequestHandler<TRequest>
where TRequest : IRequest
{
    protected TApi Api { get; }


    protected BaseIntegrationRequestHandler(TApi client)
    {
        this.Api = client ?? throw new ArgumentNullException(nameof(client));
    }


    public virtual async Task Handle(TRequest request, CancellationToken cancellationToken)
    {
        ApiResponse<object> response = null;

        try
        {
            response = await this.RequestFunc(request);

            if (response.ResultCode != "0")
                throw new IntegrationException(typeof(TApi).Name, response.ResultCode, response.ResultDescription);
        }
        catch (ApiException apiException)
        {
            throw new IntegrationException(typeof(TApi).Name, nameof(ApiException), apiException.Message, apiException);
        }
        catch (Exception ex)
        {
            throw new IntegrationException(typeof(TApi).Name, response?.ResultCode, response?.ResultDescription, ex);
        }
    }

    protected abstract Task<ApiResponse<object>> RequestFunc(TRequest request);
}


public abstract class BaseIntegrationRequestHandler<TApi, TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
where TRequest : IRequest<TResponse>
{
    protected TApi Api { get; }

    protected BaseIntegrationRequestHandler(TApi client)
    {
        this.Api = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
    {
        ApiResponse<TResponse> response = null;

        try
        {
            response = await this.RequestFunc(request);

            if (response.ResultCode != "0")
                throw new IntegrationException(typeof(TApi).Name, response.ResultCode, response.ResultDescription);
        }
        catch (ApiException apiException)
        {
            throw new IntegrationException(typeof(TApi).Name, nameof(ApiException), apiException.Message, apiException);
        }
        catch (Exception ex)
        {
            throw new IntegrationException(typeof(TApi).Name, response?.ResultCode, response?.ResultDescription, ex);
        }

        return response.Payload;
    }

    protected abstract Task<ApiResponse<TResponse>> RequestFunc(TRequest request);
}