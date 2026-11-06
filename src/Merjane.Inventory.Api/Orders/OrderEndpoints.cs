using System.Diagnostics;
using Merjane.Inventory.Application.Orders.ProcessOrder;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Merjane.Inventory.Api.Orders;

internal static class OrderEndpoints
{
    extension(IEndpointRouteBuilder endpoints)
    {
        public IEndpointRouteBuilder MapOrderEndpoints()
        {
            RouteGroupBuilder orders = endpoints.MapGroup("/orders").WithTags("Orders");

            orders.MapPost("/{orderId}/processOrder", ProcessOrderAsync)
                .WithName("ProcessOrder")
                .WithSummary("Processes an order: updates the stock of its products and notifies customers.")
                .ProducesProblem(StatusCodes.Status404NotFound);

            return endpoints;
        }
    }

    private static async Task<Results<Ok<ProcessOrderResponse>, ProblemHttpResult>> ProcessOrderAsync(
        long orderId,
        ProcessOrderHandler handler,
        CancellationToken cancellationToken)
    {
        ProcessOrderOutcome outcome = await handler.HandleAsync(orderId, cancellationToken);

        return outcome switch
        {
            ProcessOrderOutcome.Processed => TypedResults.Ok(new ProcessOrderResponse(orderId)),
            ProcessOrderOutcome.OrderNotFound => TypedResults.Problem(title: $"Order {orderId} was not found.", statusCode: StatusCodes.Status404NotFound),
            _ => throw new UnreachableException($"Unhandled outcome {outcome}."),
        };
    }
}
