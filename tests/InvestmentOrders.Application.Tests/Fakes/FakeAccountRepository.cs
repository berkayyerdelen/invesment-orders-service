using InvestmentOrders.Domain.Exceptions;
using InvestmentOrders.Domain.Models;
using InvestmentOrders.Domain.Repositories;
using InvestmentOrders.Domain.ValueObjects;

namespace InvestmentOrders.Application.Tests.Fakes;

internal sealed class FakeAccountRepository(Account account) : IAccountRepository
{
    public Account Account { get; } = account;

    public Task<Account?> GetByIdAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var result = Account.Id == accountId ? Account : null;
        return Task.FromResult(result);
    }

    public Task DebitAsync(Guid accountId, Money amount, CancellationToken cancellationToken = default)
    {
        if (Account.Id != accountId)
        {
            throw new AccountNotFoundException(accountId);
        }

        Account.Debit(amount);
        return Task.CompletedTask;
    }
}
