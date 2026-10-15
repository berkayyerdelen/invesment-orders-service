using System.ComponentModel.DataAnnotations;
using InvestmentOrders.Api.Requests;
using InvestmentOrders.Api.Responses;
using InvestmentOrders.Application.Contracts;
using InvestmentOrders.Application.Requests;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentOrders.Api.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController(IOrderService orderService) : ControllerBase
{
    /// <summary>Prices the order and checks funds without placing it — the confirmation step before execution.</summary>
    [HttpPost("validate")]
    public async Task<ActionResult<OrderValidationApiResponse>> Validate([FromBody] OrderApiRequest request, CancellationToken cancellationToken)
    {
        var result = await orderService.ValidateOrderAsync(
            new ValidateOrderRequest(request.AccountId, request.Symbol, request.Quantity), cancellationToken);

        return Ok(new OrderValidationApiResponse(result.IsValid, result.PricePerShare, result.TotalAmount, result.ErrorMessage));
    }

    /// <summary>The client generates one key per placement attempt (and reuses it on retry of that same attempt) so a resubmitted request can't double-debit the account.</summary>
    [HttpPost]
    public async Task<ActionResult<OrderApiResponse>> Place(
        [FromBody] OrderApiRequest request,
        [Required] [FromHeader(Name = "Idempotency-Key")] string idempotencyKey,
        CancellationToken cancellationToken)
    {
        var order = await orderService.PlaceOrderAsync(
            new PlaceOrderRequest(request.AccountId, request.Symbol, request.Quantity, idempotencyKey), cancellationToken);

        return Ok(new OrderApiResponse(
            order.OrderId, order.Symbol, order.Quantity, order.PricePerShare, order.TotalAmount, order.ExecutedAt, order.RemainingCashBalance));
    }
}
