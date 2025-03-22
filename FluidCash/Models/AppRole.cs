using Microsoft.AspNetCore.Identity;

namespace FluidCash.Models;

public class AppRole : IdentityRole, IBaseEntity
{
    public bool? IsDeleted { get; set; }
}