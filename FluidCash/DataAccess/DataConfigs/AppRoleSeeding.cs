using FluidCash.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace FluidCash.DataAccess.DataConfigs;

internal class AppRoleSeeding : IEntityTypeConfiguration<AppRole>
{
    public void
        Configure
        (EntityTypeBuilder<AppRole> modelBuilder)
    {
        modelBuilder.HasData
            (
                new AppRole
                {
                    Id = "PayBigiAdmin002340077770xy01",
                    Name = "AdminUser",
                    NormalizedName = "ADMINUSER",
                    IsDeleted = false,
                },
                new AppRole
                {
                    Id = "RegularUser002340077770xy01",
                    Name = "RegularUser",
                    NormalizedName = "REGULARUSER",
                    IsDeleted = false,
                }
            );
    }
}
