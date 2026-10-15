using InvestmentOrders.Application.Contracts;
using InvestmentOrders.Application.Extensions.MappingExtensions;
using InvestmentOrders.Application.Responses;
using InvestmentOrders.Domain.Exceptions;
using InvestmentOrders.Domain.Repositories;

namespace InvestmentOrders.Application.Services;

public sealed class AccountService(IAccountRepository accountRepository) : IAccountService
{
    public async Task<AccountResponse> GetAccountAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var account = await accountRepository.GetByIdAsync(accountId, cancellationToken)
            ?? throw new AccountNotFoundException(accountId);

        return account.ToResponse();
    }
}
