using InvestmentOrders.Domain.Models;
using InvestmentOrders.Domain.ValueObjects;

namespace InvestmentOrders.Domain.Repositories;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid accountId, CancellationToken cancellationToken = default);

    /// <summary>Atomically debits the account. Throws <see cref="InvestmentOrders.Domain.Exceptions.InsufficientFundsException"/> if the balance can't cover it.</summary>
    Task DebitAsync(Guid accountId, Money amount, CancellationToken cancellationToken = default);
}
