using InvestmentOrders.Application.Responses;

namespace InvestmentOrders.Application.Contracts;

public interface IAccountService
{
    Task<AccountResponse> GetAccountAsync(Guid accountId, CancellationToken cancellationToken = default);
}
