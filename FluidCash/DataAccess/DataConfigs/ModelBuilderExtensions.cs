using Microsoft.EntityFrameworkCore;

namespace FluidCash.DataAccess.DataConfigs;

public static class ModelBuilderExtensions
{
    public static void ApplyUtcToWatConversion(this ModelBuilder modelBuilder)
    {
        // Create an instance of the WAT converter
        var watConverter = new UtcToWatConverter();

        // Loop through all the entities in the model
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Loop through all the properties of the entity
            foreach (var property in entityType.GetProperties())
            {
                // Apply the conversion only to DateTime properties
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(watConverter);
                }
            }
        }
    }
}
