using InvestmentOrders.Domain.Exceptions;
using InvestmentOrders.Domain.Models;
using InvestmentOrders.Domain.Repositories;
using InvestmentOrders.Domain.ValueObjects;
using InvestmentOrders.Infrastructure.Persistence;
using Medallion.Threading;
using Microsoft.EntityFrameworkCore;

namespace InvestmentOrders.Infrastructure.Repositories;

public sealed class AccountRepository(InvestmentOrdersDbContext context, IDistributedLockProvider lockProvider) : IAccountRepository
{
    private static readonly TimeSpan LockTimeout = TimeSpan.FromSeconds(5);

    public Task<Account?> GetByIdAsync(Guid accountId, CancellationToken cancellationToken = default) =>
        context.Accounts.FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);

    public async Task<T> ExecuteWithAccountLockAsync<T>(Guid accountId, Func<Task<T>> operation, CancellationToken cancellationToken = default)
    {
        await using var handle = await lockProvider.AcquireLockAsync($"account:{accountId}", LockTimeout, cancellationToken);
        return await operation();
    }

    /// <summary>Caller must already hold <see cref="ExecuteWithAccountLockAsync{T}"/>'s lock for <paramref name="accountId"/> — that's the only concurrency guard; two debits for the same account can never race here.</summary>
    public async Task DebitAsync(Guid accountId, Money amount, CancellationToken cancellationToken = default)
    {
        var account = await context.Accounts.FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken)
            ?? throw new AccountNotFoundException(accountId);

        account.Debit(amount);

        await context.SaveChangesAsync(cancellationToken);
    }
}
