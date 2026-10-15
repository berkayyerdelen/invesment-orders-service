using Npgsql;

namespace SlotBooking.Api.Errors;

public static class PostgresErrors
{
    /// <summary>SQLSTATE raised when an EXCLUDE constraint is violated (here: an overlapping booking).</summary>
    public const string ExclusionViolation = "23P01";

    public static bool IsExclusionViolation(Exception exception) =>
        exception.InnerException is PostgresException { SqlState: ExclusionViolation };
}
