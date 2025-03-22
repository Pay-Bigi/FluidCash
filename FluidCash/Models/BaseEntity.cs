namespace FluidCash.Models;

public class BaseEntity: IBaseEntity
{
    public string? Id { get; set; } = Ulid.NewUlid().ToString();
    public string? CreatedBy { get; set; }
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
}
