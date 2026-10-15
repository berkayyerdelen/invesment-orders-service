using InvestmentOrders.Application.Responses;
using InvestmentOrders.Domain.Models;

namespace InvestmentOrders.Application.Extensions.MappingExtensions;

public static class AccountMappingExtensions
{
    public static AccountResponse ToResponse(this Account account) => new(account.Id, account.CashBalance.Amount);
}
