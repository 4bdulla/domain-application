using Domain.App.Core.Integration;
using Domain.App.Core.Integration.Attributes;

using FluentValidation;
using FluentValidation.Results;

using MediatR;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace Domain.App.Core.Utility;

internal class RouteHandlers
{
    internal const string Handler = nameof(Handler);
    internal const string HandlerWithoutValidation = nameof(HandlerWithoutValidation);


    internal class GetWithoutResponse<TRequest>
    where TRequest : IRequest
    {
        internal static Func<TRequest, IValidator<TRequest>, IMediator, CancellationToken, Task<IResult>> Handler(
            ApiRequestAttribute attribute)
            => async ([AsParameters] request, [FromServices] validator, [FromServices] mediator, token) =>
            {
                ValidationResult validationResult = await validator.ValidateAsync(request, token);

                if (!validationResult.IsValid)
                    return Results.ValidationProblem(validationResult.ToDictionary());

                await mediator.Send(request, token);

                return Results.Json(ApiResponse.Ok(), JsonHandling.Options);
            };

        internal static Func<TRequest, IMediator, CancellationToken, Task<IResult>> HandlerWithoutValidation(ApiRequestAttribute attribute)
            => async ([AsParameters] request, [FromServices] mediator, token) =>
            {
                await mediator.Send(request, token);

                return Results.Json(ApiResponse.Ok(), JsonHandling.Options);
            };
    }


    internal class GetWithResponse<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    {
        internal static Func<TRequest, IValidator<TRequest>, IMediator, CancellationToken, Task<IResult>> Handler(
            ApiRequestAttribute attribute)
            => async ([AsParameters] request, [FromServices] validator, [FromServices] mediator, token) =>
            {
                ValidationResult validationResult = await validator.ValidateAsync(request, token);

                return validationResult.IsValid
                    ? Results.Json(ApiResponse.Ok(await mediator.Send(request, token)), JsonHandling.Options)
                    : Results.ValidationProblem(validationResult.ToDictionary());
            };

        internal static Func<TRequest, IMediator, CancellationToken, Task<IResult>> HandlerWithoutValidation(ApiRequestAttribute attribute)
            => async ([AsParameters] request, [FromServices] mediator, token) =>
            Results.Json(ApiResponse.Ok(await mediator.Send(request, token)), JsonHandling.Options);
    }


    internal class PostPutDeleteWithoutResponse<TRequest>
    where TRequest : IRequest
    {
        internal static Func<TRequest, IValidator<TRequest>, IMediator, CancellationToken, Task<IResult>> Handler(
            ApiRequestAttribute attribute) =>
            async ([FromBody] request, [FromServices] validator, [FromServices] mediator, token) =>
            {
                ValidationResult validationResult = await validator.ValidateAsync(request, token);

                if (!validationResult.IsValid)
                    return Results.ValidationProblem(validationResult.ToDictionary());

                await mediator.Send(request, token);

                return Results.Json(ApiResponse.Ok(), JsonHandling.Options);
            };

        internal static Func<TRequest, IMediator, CancellationToken, Task<IResult>> HandlerWithoutValidation(ApiRequestAttribute attribute)
            => async ([FromBody] request, [FromServices] mediator, token) =>
            {
                await mediator.Send(request, token);

                return Results.Json(ApiResponse.Ok(), JsonHandling.Options);
            };
    }


    internal class PostPutDeleteWithResponse<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    {
        internal static Func<TRequest, IValidator<TRequest>, IMediator, CancellationToken, Task<IResult>> Handler(
            ApiRequestAttribute attribute) =>
            async ([FromBody] request, [FromServices] validator, [FromServices] mediator, token) =>
            {
                ValidationResult validationResult = await validator.ValidateAsync(request, token);

                return validationResult.IsValid
                    ? Results.Json(ApiResponse.Ok(await mediator.Send(request, token)), JsonHandling.Options)
                    : Results.ValidationProblem(validationResult.ToDictionary());
            };

        internal static Func<TRequest, IMediator, CancellationToken, Task<IResult>> HandlerWithoutValidation(ApiRequestAttribute attribute)
            => async ([FromBody] request, [FromServices] mediator, token) =>
            Results.Json(ApiResponse.Ok(await mediator.Send(request, token)), JsonHandling.Options);
    }
}