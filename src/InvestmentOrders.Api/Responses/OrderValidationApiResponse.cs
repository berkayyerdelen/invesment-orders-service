namespace InvestmentOrders.Api.Responses;

public sealed record OrderValidationApiResponse(bool IsValid, decimal PricePerShare, decimal TotalAmount, string? ErrorMessage);
