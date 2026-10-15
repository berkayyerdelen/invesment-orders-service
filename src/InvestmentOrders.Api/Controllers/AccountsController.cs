using InvestmentOrders.Api.Responses;
using InvestmentOrders.Application.Contracts;
using InvestmentOrders.Infrastructure.Configurations;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentOrders.Api.Controllers;

[ApiController]
[Route("api/accounts")]
public sealed class AccountsController(IAccountService accountService) : ControllerBase
{
    [HttpGet("default")]
    public async Task<ActionResult<AccountApiResponse>> GetDefault(CancellationToken cancellationToken)
    {
        var account = await accountService.GetAccountAsync(SeedData.DefaultAccountId, cancellationToken);
        return Ok(new AccountApiResponse(account.AccountId, account.CashBalance));
    }
}
