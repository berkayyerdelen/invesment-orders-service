using InvestmentOrders.SharedKernel.Exceptions;

namespace InvestmentOrders.Domain.Exceptions;

public sealed class AccountNotFoundException(Guid accountId)
    : DomainException("Account not found", $"No investment account found with id '{accountId}'.", 404);
