using System.ComponentModel.DataAnnotations;

namespace InvestmentOrders.Api.Requests;

/// <summary>Shared shape for both /orders/validate (preview) and POST /orders (execute) — same inputs, different outcome.</summary>
public sealed class OrderApiRequest
{
    [Required]
    public Guid AccountId { get; init; }

    [Required(AllowEmptyStrings = false)]
    public string Symbol { get; init; } = string.Empty;

    [Range(1, 1_000_000, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; init; }
}
