namespace InvestmentOrders.SharedKernel.Exceptions;

/// <summary>Expected, user-facing failure. The API's global exception handler maps each one to a ProblemDetails response.</summary>
public abstract class DomainException(string title, string message, int statusCode) : Exception(message)
{
    public string Title { get; } = title;
    public int StatusCode { get; } = statusCode;
}
