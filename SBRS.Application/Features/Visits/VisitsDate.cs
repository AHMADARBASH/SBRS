namespace SBRS.Application.Features.Visits;

/// <summary>Today in the beneficiary's time zone, shared by validators and handlers.</summary>
public static class VisitsDate
{
    private static readonly TimeZoneInfo TimeZone = Resolve();

    public static DateOnly Today =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZone));

    private static TimeZoneInfo Resolve()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("Asia/Damascus"); }
        catch (TimeZoneNotFoundException)
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById("Syria Standard Time"); }
            catch (TimeZoneNotFoundException) { return TimeZoneInfo.Utc; }
        }
        catch (InvalidTimeZoneException) { return TimeZoneInfo.Utc; }
    }
}
