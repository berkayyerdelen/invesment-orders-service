using InvestmentOrders.Api.Responses;
using InvestmentOrders.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentOrders.Api.Controllers;

[ApiController]
[Route("api/stocks")]
public sealed class StocksController(IStockService stockService) : ControllerBase
{
    [HttpGet("{symbol}")]
    public async Task<ActionResult<StockQuoteApiResponse>> GetQuote(string symbol, CancellationToken cancellationToken)
    {
        var quote = await stockService.GetQuoteAsync(symbol, cancellationToken);
        return Ok(new StockQuoteApiResponse(quote.Symbol, quote.Name, quote.Price));
    }
}
