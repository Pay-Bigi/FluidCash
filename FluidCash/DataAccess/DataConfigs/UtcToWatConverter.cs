using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FluidCash.DataAccess.DataConfigs;

public class UtcToWatConverter : ValueConverter<DateTime, DateTime>
{
    private static readonly TimeZoneInfo WatTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Central Africa Standard Time");

    public UtcToWatConverter()
        : base(
            v => v, // Store in UTC (no change when saving)
            v => TimeZoneInfo.ConvertTimeFromUtc(v, WatTimeZone) // Convert from UTC to WAT when reading
        )
    { }
}