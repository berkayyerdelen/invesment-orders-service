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
    private const int MaxConcurrencyAttempts = 3;

    public Task<Account?> GetByIdAsync(Guid accountId, CancellationToken cancellationToken = default) =>
        context.Accounts.FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);

    public async Task DebitAsync(Guid accountId, Money amount, CancellationToken cancellationToken = default)
    {
        await using var handle = await lockProvider.AcquireLockAsync($"account:{accountId}", LockTimeout, cancellationToken);

        for (var attempt = 1; ; attempt++)
        {
            var account = await context.Accounts.FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken)
                ?? throw new AccountNotFoundException(accountId);

            account.Debit(amount);

            try
            {
                await context.SaveChangesAsync(cancellationToken);
                return;
            }
            catch (DbUpdateConcurrencyException) when (attempt < MaxConcurrencyAttempts)
            {
                // The Redis lock should make this rare in practice; this is the DB-level backstop for any race it didn't catch.
                context.ChangeTracker.Clear();
            }
        }
    }
}
