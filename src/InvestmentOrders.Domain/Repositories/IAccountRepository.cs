using InvestmentOrders.Domain.Models;
using InvestmentOrders.Domain.ValueObjects;

namespace InvestmentOrders.Domain.Repositories;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid accountId, CancellationToken cancellationToken = default);

    /// <summary>Atomically debits the account. Throws <see cref="InvestmentOrders.Domain.Exceptions.InsufficientFundsException"/> if the balance can't cover it. Callers that also need the idempotency check and the debit to be one atomic unit should run both inside <see cref="ExecuteWithAccountLockAsync{T}"/>.</summary>
    Task DebitAsync(Guid accountId, Money amount, CancellationToken cancellationToken = default);

    /// <summary>Runs <paramref name="operation"/> while holding an exclusive lock scoped to <paramref name="accountId"/>, serializing it against every other call for the same account (including <see cref="DebitAsync"/>).</summary>
    Task<T> ExecuteWithAccountLockAsync<T>(Guid accountId, Func<Task<T>> operation, CancellationToken cancellationToken = default);
}
