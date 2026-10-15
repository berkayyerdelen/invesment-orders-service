namespace InvestmentOrders.Application.Responses;

public sealed record OrderValidationResponse(bool IsValid, decimal PricePerShare, decimal TotalAmount, string? ErrorMessage);
